using wdb_backend.Abstractions;
using wdb_backend.Common;
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
        var approvedPermissions = await _permissionService.GetAllByWorkerIdAsync(workerId, PermissionStatus.Approved);
        var workerInfos = await _workerInfoService.GetAllAsync(workerId);
        var now = DateTime.UtcNow;

        // ExpiryDate is now on Request, not Permission — filter by request expiry
        var activePermissions = approvedPermissions
            .Where(p =>
            {
                var req = requests.FirstOrDefault(r => r.Id == p.RequestId);
                return req != null && req.ExpiryDate > now;
            })
            .ToList();

        var rows = new List<ActiveAccessDto>();

        foreach (var request in requests)
        {
            var permsForRequest = activePermissions
                .Where(p => p.RequestId == request.Id)
                .ToList();

            if (!permsForRequest.Any()) continue;

            var employer = await _employerService.GetEmployerInfoAsync(request.EmployerId);
            var companyName = employer?.Name ?? "Unknown";

            if (!string.IsNullOrWhiteSpace(company) &&
                !companyName.Contains(company, StringComparison.OrdinalIgnoreCase))
                continue;

            var infoItems = permsForRequest
                .Select(p =>
                {
                    var info = workerInfos.FirstOrDefault(w => w.Id == p.InfoId);
                    // Use CustomLabel for custom fields, Field navigation label for preset fields
                    var label = info?.CustomLabel ?? info?.Field?.Label ?? "Unknown";
                    return new ActiveAccessInfoDto
                    {
                        PermissionId = p.Id,
                        DataType = label
                    };
                })
                .ToList();

            if (!string.IsNullOrWhiteSpace(dataType))
            {
                infoItems = infoItems
                    .Where(i => i.DataType.Equals(dataType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!infoItems.Any()) continue;
            }

            rows.Add(new ActiveAccessDto
            {
                RequestId = request.Id,
                CompanyName = companyName,
                GrantedAt = permsForRequest.Max(p => p.LastUpdatedAt) ?? DateTime.UtcNow,
                Reason = request.Reason,
                WorkerInfo = infoItems
            });
        }

        return rows.OrderByDescending(r => r.GrantedAt).ToList();
    }
}
