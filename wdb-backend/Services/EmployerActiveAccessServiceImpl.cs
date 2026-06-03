using MediatR;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerActiveAccessServiceImpl : IEmployerActiveAccessService
{
    private readonly AppDbContext _context;
    private readonly ISupabaseStorageService _storage;
    private readonly IMediator _mediator;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<EmployerActiveAccessServiceImpl> _logger;

    public EmployerActiveAccessServiceImpl(
        AppDbContext context,
        ISupabaseStorageService storage,
        IMediator mediator,
        IBlockchainService blockchainService,
        ILogger<EmployerActiveAccessServiceImpl> logger)
    {
        _context = context;
        _storage = storage;
        _mediator = mediator;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<List<EmployerActiveAccessDto>> GetActiveAccessAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var employerExists = await _context.Employers
            .AsNoTracking()
            .AnyAsync(e => e.Id == employerId, cancellationToken);

        if (!employerExists)
        {
            throw new UnauthorizedAccessException("Current user is not an employer.");
        }

        var now = DateTime.UtcNow;

        var requests = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Worker)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo!)
                    .ThenInclude(wi => wi.Field!)
                        .ThenInclude(f => f.Category)
            .Where(r => r.EmployerId == employerId && r.ExpiryDate > now)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<EmployerActiveAccessDto>();

        foreach (var request in requests)
        {
            var approvedPerms = request.Permissions
                .Where(p => p.Status == PermissionStatus.Approved && p.WorkerInfo != null)
                .ToList();

            if (approvedPerms.Count == 0) continue;

            var groups = approvedPerms
                .Select(p => new
                {
                    Permission = p,
                    CategoryName = p.WorkerInfo!.Field?.Category?.CategoryName
                                   ?? "OtherInformation",
                    Label = p.WorkerInfo.CustomLabel
                            ?? p.WorkerInfo.Field?.Label
                            ?? "Unknown",
                    IsCustom = p.WorkerInfo.CustomLabel != null
                })
                .GroupBy(x => x.CategoryName)
                .Select(g => new EmployerActiveAccessCategoryDto
                {
                    Name = g.Key,
                    Items = g.Select(x => new EmployerActiveAccessItemDto
                    {
                        PermissionId = x.Permission.Id,
                        Label = x.Label,
                        Type = x.Permission.WorkerInfo!.Type,
                        IsCustom = x.IsCustom
                    }).ToList()
                })
                .ToList();

            result.Add(new EmployerActiveAccessDto
            {
                RequestId = request.Id,
                WorkerId = request.WorkerId,
                WorkerName = request.Worker.Name,
                WorkerEmail = request.Worker.Email,
                Reason = request.Reason,
                GrantedAt = approvedPerms.Max(p => p.LastUpdatedAt) ?? request.CreatedAt,
                ExpiryDate = request.ExpiryDate,
                Categories = groups
            });
        }

        return result;
    }

    public async Task<EmployerAccessViewResultDto> ViewAsync(
        Guid employerId,
        Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions
            .AsNoTracking()
            .Include(p => p.Request)
            .Include(p => p.Field)
                .ThenInclude(f => f!.Category)
            .Include(p => p.WorkerInfo)
                .ThenInclude(wi => wi!.Field)
                    .ThenInclude(f => f!.Category)
            .FirstOrDefaultAsync(p => p.Id == permissionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Permission {permissionId} not found");

        if (permission.Request.EmployerId != employerId)
        {
            throw new UnauthorizedAccessException("Permission does not belong to the current employer.");
        }

        if (permission.Status != PermissionStatus.Approved)
        {
            throw new InvalidOperationException("Permission is not approved.");
        }

        if (permission.Request.ExpiryDate <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Permission has expired.");
        }

        if (permission.WorkerInfo == null)
        {
            throw new InvalidOperationException("No data associated with this permission yet.");
        }

        var info = permission.WorkerInfo;

        await _mediator.Send(
            new NotificationCommand(
                EmployerId: permission.Request.EmployerId,
                WorkerId: permission.WorkerId,
                RequestId: permission.RequestId,
                FieldLabel: ResolveLabel(permission),
                Type: NotificationType.DataAccessed),
            cancellationToken);

        await LogDataViewedToBlockchainAsync(
            employerId,
            permission,
            cancellationToken);

        if (info.Type == "file")
        {
            if (string.IsNullOrWhiteSpace(info.Value))
            {
                throw new InvalidOperationException("No file path stored for this item.");
            }

            var signed = await _storage.CreateSignedUrlAsync(info.Value, 900, cancellationToken);

            return new EmployerAccessViewResultDto
            {
                Type = "file",
                Url = signed.Url,
                UrlExpiresAt = signed.ExpiresAt
            };
        }

        return new EmployerAccessViewResultDto
        {
            Type = "text",
            Value = info.Value
        };
    }

    private async Task LogDataViewedToBlockchainAsync(
        Guid employerId,
        Permission permission,
        CancellationToken cancellationToken)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken)
            ?? throw new KeyNotFoundException("EMPLOYER_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

        var worker = await _context.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == permission.WorkerId, cancellationToken)
            ?? throw new KeyNotFoundException("WORKER_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

        if (string.IsNullOrWhiteSpace(employer.PrivateKey))
            throw new InvalidOperationException("EMPLOYER_PRIVATE_KEY_MISSING");

        if (string.IsNullOrWhiteSpace(employer.BlockchainAddress))
            throw new InvalidOperationException("EMPLOYER_BLOCKCHAIN_ADDRESS_MISSING");

        if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
            throw new InvalidOperationException("WORKER_BLOCKCHAIN_ADDRESS_MISSING");

        var category = ResolveCategory(permission);
        var label = ResolveLabel(permission);

        _logger.LogWarning(
            "Writing employer data-view blockchain log. RequestId={RequestId}, PermissionId={PermissionId}, Category={Category}, Label={Label}",
            permission.RequestId,
            permission.Id,
            category,
            label);

        var txHash = await _blockchainService.LogCategoryTransactionAsync(
            privateKey: employer.PrivateKey!,
            employerAddress: employer.BlockchainAddress!,
            workerAddress: worker.BlockchainAddress!,
            requestId: permission.RequestId.ToString(),
            category: category,
            permissionIds: permission.Id.ToString(),
            itemLabels: label,
            action: BlockchainAction.DataViewed,
            cancellationToken: cancellationToken);

        _logger.LogWarning(
            "Employer data-view blockchain log written successfully. RequestId={RequestId}, PermissionId={PermissionId}, TxHash={TxHash}",
            permission.RequestId,
            permission.Id,
            txHash);
    }

    private static string ResolveLabel(Permission permission)
    {
        return permission.Field?.Label
            ?? permission.WorkerInfo?.Field?.Label
            ?? permission.WorkerInfo?.CustomLabel
            ?? "Unknown";
    }

    private static string ResolveCategory(Permission permission)
    {
        return permission.Field?.Category?.CategoryName
            ?? permission.WorkerInfo?.Field?.Category?.CategoryName
            ?? "OtherInformation";
    }
}
