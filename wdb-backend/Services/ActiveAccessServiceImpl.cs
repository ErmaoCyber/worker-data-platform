using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class ActiveAccessServiceImpl : IActiveAccessService
{
    private readonly IPermissionService _permissionService;
    private readonly IRequestService _requestService;
    private readonly IWorkerInfoService _workerInfoService;
    private readonly IEmployerService _employerService;

    public ActiveAccessServiceImpl(
        IPermissionService permissionService,
        IRequestService requestService,
        IWorkerInfoService workerInfoService,
        IEmployerService employerService)
    {
        _permissionService = permissionService;
        _requestService = requestService;
        _workerInfoService = workerInfoService;
        _employerService = employerService;
    }

    public async Task<List<ActiveAccessDto>> GetActiveAccessAsync(
        Guid workerId,
        string? company = null,
        string? dataType = null)
    {
        var requests = await _requestService.GetAllByWorkerIdAsync(workerId);
        var approvedPermissions = await _permissionService.GetAllByWorkerIdAsync(workerId, 1);
        var workerInfo = await _workerInfoService.GetAllAsync(workerId);

        var now = DateTime.UtcNow;

        var activePermissions = approvedPermissions
            .Where(permission =>
                !permission.ExpiryDate.HasValue ||
                permission.ExpiryDate.Value > now)
            .ToList();

        var rows = new List<ActiveAccessDto>();

        foreach (var request in requests)
        {
            var permissionsForRequest = activePermissions
                .Where(permission => permission.RequestId == request.Id)
                .ToList();

            if (!permissionsForRequest.Any())
            {
                continue;
            }

            var employer = await _employerService.GetEmployerInfoAsync(request.EmployerId);
            var companyName = employer?.Name ?? "Unknown";

            if (!string.IsNullOrWhiteSpace(company) &&
                !companyName.Contains(company, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var infoItems = permissionsForRequest
                .Select(permission =>
                {
                    var info = workerInfo.FirstOrDefault(workerInfoItem =>
                        workerInfoItem.Id == permission.InfoId);

                    return new ActiveAccessInfoDto
                    {
                        PermissionId = permission.Id,
                        DataType = info?.Desc ?? "Unknown"
                    };
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(dataType))
            {
                infoItems = infoItems
                    .Where(info =>
                        info.DataType.Equals(dataType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!infoItems.Any())
                {
                    continue;
                }
            }

            rows.Add(new ActiveAccessDto
            {
                RequestId = request.Id,
                CompanyName = companyName,
                GrantedAt = permissionsForRequest.Max(permission => permission.LastUpdatedAt),
                Reason = request.Reason,
                WorkerInfo = infoItems
            });
        }

        return rows
            .OrderByDescending(row => row.GrantedAt)
            .ToList();
    }
}
