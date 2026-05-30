//using Moq;
//using Xunit;
//using wdb_backend.Models;
//using wdb_backend.Abstractions;
//using wdb_backend.Common;
//using wdb_backend.Services;

//namespace wdb_backend.Tests;

//public class ActiveAccessRowTest
//{
//    private static ActiveAccessServiceImpl CreateService(
//        Mock<IPermissionService> permissionService,
//        Mock<IRequestService> requestService,
//        Mock<IWorkerInfoService> workerInfoService,
//        Mock<IEmployerService> employerService)
//    {
//        return new ActiveAccessServiceImpl(
//            permissionService.Object,
//            requestService.Object,
//            workerInfoService.Object,
//            employerService.Object
//        );
//    }

//    // Request with no approved permissions is skipped

//    [Fact]
//    public async Task GetActiveAccess_RequestWithNoApprovedPermissions_IsSkipped()
//    {
//        var workerId = Guid.NewGuid();
//        var requestWithPerms = Guid.NewGuid();
//        var requestWithout = Guid.NewGuid();
//        var employerId = Guid.NewGuid();
//        var permissionId = Guid.NewGuid();
//        var infoId = Guid.NewGuid();

//        var mockPermissionService = new Mock<IPermissionService>();
//        var mockRequestService = new Mock<IRequestService>();
//        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
//        var mockEmployerService = new Mock<IEmployerService>();

//        mockRequestService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
//            .ReturnsAsync(new List<Request>
//            {
//                new Request { Id = requestWithPerms, EmployerId = employerId, WorkerId = workerId, Reason = "Has perms", CreatedAt = DateTime.UtcNow },
//                new Request { Id = requestWithout,   EmployerId = employerId, WorkerId = workerId, Reason = "No perms",  CreatedAt = DateTime.UtcNow }
//            });

//        mockPermissionService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
//            .ReturnsAsync(new List<Permission>
//            {
//                new Permission
//                {
//                    Id = permissionId,
//                    WorkerId = workerId,
//                    RequestId = requestWithPerms,
//                    InfoId = infoId,
//                    Status = (PermissionStatus)1,
//                    LastUpdatedAt = DateTime.UtcNow,
//                    ExpiryDate = DateTime.UtcNow.AddDays(7)
//                }
//            });

//        mockWorkerInfoService
//            .Setup(s => s.GetAllAsync(workerId))
//            .ReturnsAsync(new List<WorkerInfo>
//            {
//                new WorkerInfo
//                {
//                    Id = infoId,
//                    WorkerId = workerId,
//                    Desc = "country",
//                    Value = "New Zealand"
//                }
//            });

//        mockEmployerService
//            .Setup(s => s.GetEmployerInfoAsync(employerId))
//            .ReturnsAsync(new Employer
//            {
//                Id = employerId,
//                Name = "Acme Corp",
//                Email = "acme@example.com",
//                Password = "password"
//            });

//        var service = CreateService(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

//        // Act
//        var rows = await service.GetActiveAccessAsync(workerId);

//        // Assert
//        Assert.Single(rows);
//        Assert.Equal("Has perms", rows[0].Reason);
//        Assert.Equal(requestWithPerms, rows[0].RequestId);
//        Assert.Equal("Acme Corp", rows[0].CompanyName);
//    }

//    //All requests have no approved permissions

//    [Fact]
//    public async Task GetActiveAccess_AllRequestsHaveNoApprovedPermissions_ReturnsEmptyList()
//    {
//        var workerId = Guid.NewGuid();
//        var employerId = Guid.NewGuid();

//        var mockPermissionService = new Mock<IPermissionService>();
//        var mockRequestService = new Mock<IRequestService>();
//        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
//        var mockEmployerService = new Mock<IEmployerService>();

//        mockRequestService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
//            .ReturnsAsync(new List<Request>
//            {
//                new Request { Id = Guid.NewGuid(), EmployerId = employerId, WorkerId = workerId, Reason = "Reason 1", CreatedAt = DateTime.UtcNow },
//                new Request { Id = Guid.NewGuid(), EmployerId = employerId, WorkerId = workerId, Reason = "Reason 2", CreatedAt = DateTime.UtcNow }
//            });

//        mockPermissionService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
//            .ReturnsAsync(new List<Permission>());

//        mockWorkerInfoService
//            .Setup(s => s.GetAllAsync(workerId))
//            .ReturnsAsync(new List<WorkerInfo>());

//        var service = CreateService(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

//        // Act
//        var rows = await service.GetActiveAccessAsync(workerId);

//        // Assert
//        Assert.Empty(rows);
//    }

//    // WorkerInfo not found → Label is "Unknown" 

//    [Fact]
//    public async Task GetActiveAccess_WorkerInfoNotFound_LabelIsUnknown()
//    {
//        // Arrange
//        var workerId = Guid.NewGuid();
//        var requestId = Guid.NewGuid();
//        var employerId = Guid.NewGuid();

//        var mockPermissionService = new Mock<IPermissionService>();
//        var mockRequestService = new Mock<IRequestService>();
//        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
//        var mockEmployerService = new Mock<IEmployerService>();

//        mockRequestService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
//            .ReturnsAsync(new List<Request>
//            {
//                new Request { Id = requestId, EmployerId = employerId, WorkerId = workerId, Reason = "Reason", CreatedAt = DateTime.UtcNow }
//            });

//        mockPermissionService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
//            .ReturnsAsync(new List<Permission>
//            {
//                new Permission
//                {
//                    Id = Guid.NewGuid(),
//                    WorkerId = workerId,
//                    RequestId = requestId,
//                    InfoId = Guid.NewGuid(),
//                    Status = (PermissionStatus)1,
//                    LastUpdatedAt = DateTime.UtcNow,
//                    ExpiryDate = DateTime.UtcNow.AddDays(7)
//                }
//            });

//        // empty — InfoId won't match anything
//        mockWorkerInfoService
//            .Setup(s => s.GetAllAsync(workerId))
//            .ReturnsAsync(new List<WorkerInfo>());

//        mockEmployerService
//            .Setup(s => s.GetEmployerInfoAsync(employerId))
//            .ReturnsAsync(new Employer
//            {
//                Id = employerId,
//                Name = "Acme Corp",
//                Email = "acme@example.com",
//                Password = "password"
//            });

//        var service = CreateService(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

//        // Act
//        var rows = await service.GetActiveAccessAsync(workerId);

//        // Assert
//        Assert.Equal("Unknown", rows[0].WorkerInfo[0].DataType);
//    }

//    //Employer not found → Company is "Unknown"

//    [Fact]
//    public async Task GetActiveAccess_EmployerNotFound_CompanyIsUnknown()
//    {
//        // Arrange
//        var workerId = Guid.NewGuid();
//        var requestId = Guid.NewGuid();
//        var employerId = Guid.NewGuid();

//        var mockPermissionService = new Mock<IPermissionService>();
//        var mockRequestService = new Mock<IRequestService>();
//        var mockWorkerInfoService = new Mock<IWorkerInfoService>();
//        var mockEmployerService = new Mock<IEmployerService>();

//        mockRequestService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId))
//            .ReturnsAsync(new List<Request>
//            {
//                new Request { Id = requestId, EmployerId = employerId, WorkerId = workerId, Reason = "Reason", CreatedAt = DateTime.UtcNow }
//            });

//        mockPermissionService
//            .Setup(s => s.GetAllByWorkerIdAsync(workerId, 1))
//            .ReturnsAsync(new List<Permission>
//            {
//                new Permission
//                {
//                    Id = Guid.NewGuid(),
//                    WorkerId = workerId,
//                    RequestId = requestId,
//                    Status = (PermissionStatus)1,
//                    LastUpdatedAt = DateTime.UtcNow,
//                    ExpiryDate = DateTime.UtcNow.AddDays(7)
//                }
//            });

//        mockWorkerInfoService
//            .Setup(s => s.GetAllAsync(workerId))
//            .ReturnsAsync(new List<WorkerInfo>());

//        // employer returns null
//        mockEmployerService
//            .Setup(s => s.GetEmployerInfoAsync(employerId))
//            .ReturnsAsync((Employer?)null);

//        var service = CreateService(mockPermissionService, mockRequestService, mockWorkerInfoService, mockEmployerService);

//        // Act
//        var rows = await service.GetActiveAccessAsync(workerId);

//        // Assert
//        Assert.Equal("Unknown", rows[0].CompanyName);
//    }
//}
