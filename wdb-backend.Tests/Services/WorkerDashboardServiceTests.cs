//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Moq;
//using wdb_backend.Abstractions;
//using wdb_backend.Common;
//using wdb_backend.Data;
//using wdb_backend.Models;
//using wdb_backend.Services;

//namespace wdb_backend.Tests.Services;

//public class WorkerDashboardServiceTests
//{
//    private const string TestPassword = "Password123!";

//    private static AppDbContext CreateDbContext()
//    {
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(Guid.NewGuid().ToString())
//            .Options;

//        return new AppDbContext(options);
//    }

//    private static WorkerDashboardServiceImpl CreateService(
//        AppDbContext dbContext,
//        Mock<IBlockchainService>? blockchainServiceMock = null)
//    {
//        blockchainServiceMock ??= new Mock<IBlockchainService>();

//        var loggerMock = new Mock<ILogger<WorkerDashboardServiceImpl>>();

//        return new WorkerDashboardServiceImpl(
//            dbContext,
//            blockchainServiceMock.Object,
//            loggerMock.Object
//        );
//    }

//    [Fact]
//    public async Task GetDashboardAsync_ReturnsNull_WhenWorkerDoesNotExist()
//    {
//        // Arrange
//        await using var dbContext = CreateDbContext();
//        var service = CreateService(dbContext);

//        var missingWorkerId = Guid.NewGuid();

//        // Act
//        var result = await service.GetDashboardAsync(missingWorkerId);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetDashboardAsync_ReturnsWorkerBasicInfo_WhenWorkerExists()
//    {
//        // Arrange
//        await using var dbContext = CreateDbContext();

//        var worker = new Worker
//        {
//            Id = Guid.NewGuid(),
//            Name = "Alan Brown",
//            Email = "worker_001@test.com",
//            Password = TestPassword,
//            Verified = false,
//            BlockchainAddress = null
//        };

//        dbContext.Workers.Add(worker);
//        await dbContext.SaveChangesAsync();

//        var service = CreateService(dbContext);

//        // Act
//        var result = await service.GetDashboardAsync(worker.Id);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(worker.Id, result.Worker.Id);
//        Assert.Equal("Alan Brown", result.Worker.Name);
//        Assert.Equal("worker_001@test.com", result.Worker.Email);
//        Assert.False(result.Worker.Verified);
//        Assert.Null(result.Worker.BlockchainAddress);
//    }

//    [Fact]
//    public async Task GetDashboardAsync_ReturnsLatestRequests_WithPermissionAndWorkerInfoData()
//    {
//        // Arrange
//        await using var dbContext = CreateDbContext();

//        var workerId = Guid.NewGuid();
//        var employerId = Guid.NewGuid();
//        var requestId = Guid.NewGuid();
//        var workerInfoId = Guid.NewGuid();

//        var worker = new Worker
//        {
//            Id = workerId,
//            Name = "Alan Brown",
//            Email = "worker_001@test.com",
//            Password = TestPassword,
//            Verified = false,
//            BlockchainAddress = null
//        };

//        var employer = new Employer
//        {
//            Id = employerId,
//            Name = "BuildSafe Ltd",
//            Email = "buildsafe@test.com",
//            Password = TestPassword,
//            Verified = true
//        };

//        var workerInfo = new WorkerInfo
//        {
//            Id = workerInfoId,
//            WorkerId = workerId,
//            Desc = "PPE requirements",
//            Value = "Safety boots required"
//        };

//        var request = new Request
//        {
//            Id = requestId,
//            WorkerId = workerId,
//            EmployerId = employerId,
//            Reason = "Site onboarding check",
//            CreatedAt = new DateTime(2026, 5, 14, 10, 30, 0, DateTimeKind.Utc)
//        };

//        var permission = new Permission
//        {
//            Id = Guid.NewGuid(),
//            WorkerId = workerId,
//            RequestId = requestId,
//            InfoId = workerInfoId,
//            Status = PermissionStatus.Pending,
//            ExpiryDate = null,
//            LastUpdatedAt = new DateTime(2026, 5, 14, 10, 30, 0, DateTimeKind.Utc)
//        };

//        dbContext.Workers.Add(worker);
//        dbContext.Employers.Add(employer);
//        dbContext.WorkerInfos.Add(workerInfo);
//        dbContext.Requests.Add(request);
//        dbContext.Permissions.Add(permission);

//        await dbContext.SaveChangesAsync();

//        var service = CreateService(dbContext);

//        // Act
//        var result = await service.GetDashboardAsync(workerId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Single(result.LatestRequests);

//        var latestRequest = result.LatestRequests[0];

//        Assert.Equal(requestId, latestRequest.RequestId);
//        Assert.Equal(employerId, latestRequest.EmployerId);
//        Assert.Equal("BuildSafe Ltd", latestRequest.EmployerName);
//        Assert.Equal("PPE requirements", latestRequest.RequestedInformation);
//        Assert.Equal("Site onboarding check", latestRequest.CheckPurpose);
//        Assert.Equal(request.CreatedAt, latestRequest.CreatedAt);
//        Assert.Equal((int)PermissionStatus.Pending, latestRequest.Status);
//        Assert.Null(latestRequest.ExpiresAt);
//    }

//    [Fact]
//    public async Task GetDashboardAsync_ReturnsApprovedRequest_WithExpiryDate()
//    {
//        // Arrange
//        await using var dbContext = CreateDbContext();

//        var workerId = Guid.NewGuid();
//        var employerId = Guid.NewGuid();
//        var requestId = Guid.NewGuid();
//        var workerInfoId = Guid.NewGuid();

//        var expiryDate = new DateTime(2026, 6, 6, 0, 0, 0, DateTimeKind.Utc);

//        dbContext.Workers.Add(new Worker
//        {
//            Id = workerId,
//            Name = "Liam Johnson",
//            Email = "liam.johnson@example.com",
//            Password = TestPassword,
//            Verified = false,
//            BlockchainAddress = null
//        });

//        dbContext.Employers.Add(new Employer
//        {
//            Id = employerId,
//            Name = "Company_D",
//            Email = "companyd@test.com",
//            Password = TestPassword,
//            Verified = true
//        });

//        dbContext.WorkerInfos.Add(new WorkerInfo
//        {
//            Id = workerInfoId,
//            WorkerId = workerId,
//            Desc = "Family health history",
//            Value = "Private test value"
//        });

//        dbContext.Requests.Add(new Request
//        {
//            Id = requestId,
//            WorkerId = workerId,
//            EmployerId = employerId,
//            Reason = "Pre-employment screening",
//            CreatedAt = new DateTime(2026, 5, 6, 2, 15, 0, DateTimeKind.Utc)
//        });

//        dbContext.Permissions.Add(new Permission
//        {
//            Id = Guid.NewGuid(),
//            WorkerId = workerId,
//            RequestId = requestId,
//            InfoId = workerInfoId,
//            Status = PermissionStatus.Approved,
//            ExpiryDate = expiryDate,
//            LastUpdatedAt = new DateTime(2026, 5, 6, 2, 20, 0, DateTimeKind.Utc)
//        });

//        await dbContext.SaveChangesAsync();

//        var service = CreateService(dbContext);

//        // Act
//        var result = await service.GetDashboardAsync(workerId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Single(result.LatestRequests);

//        var latestRequest = result.LatestRequests[0];

//        Assert.Equal("Company_D", latestRequest.EmployerName);
//        Assert.Equal("Family health history", latestRequest.RequestedInformation);
//        Assert.Equal("Pre-employment screening", latestRequest.CheckPurpose);
//        Assert.Equal((int)PermissionStatus.Approved, latestRequest.Status);
//        Assert.Equal(expiryDate, latestRequest.ExpiresAt);
//    }

//    [Fact]
//    public async Task GetDashboardAsync_ReturnsOnlyLatestFiveRequests()
//    {
//        // Arrange
//        await using var dbContext = CreateDbContext();

//        var workerId = Guid.NewGuid();
//        var employerId = Guid.NewGuid();

//        dbContext.Workers.Add(new Worker
//        {
//            Id = workerId,
//            Name = "Alan Brown",
//            Email = "worker_001@test.com",
//            Password = TestPassword,
//            Verified = false,
//            BlockchainAddress = null
//        });

//        dbContext.Employers.Add(new Employer
//        {
//            Id = employerId,
//            Name = "Company_001",
//            Email = "company001@test.com",
//            Password = TestPassword,
//            Verified = true
//        });

//        for (var i = 1; i <= 6; i++)
//        {
//            var requestId = Guid.NewGuid();
//            var workerInfoId = Guid.NewGuid();

//            dbContext.WorkerInfos.Add(new WorkerInfo
//            {
//                Id = workerInfoId,
//                WorkerId = workerId,
//                Desc = $"Info {i}",
//                Value = $"Value {i}"
//            });

//            dbContext.Requests.Add(new Request
//            {
//                Id = requestId,
//                WorkerId = workerId,
//                EmployerId = employerId,
//                Reason = $"Reason {i}",
//                CreatedAt = new DateTime(2026, 5, i, 10, 0, 0, DateTimeKind.Utc)
//            });

//            dbContext.Permissions.Add(new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = requestId,
//                InfoId = workerInfoId,
//                Status = PermissionStatus.Pending,
//                ExpiryDate = null,
//                LastUpdatedAt = new DateTime(2026, 5, i, 10, 0, 0, DateTimeKind.Utc)
//            });
//        }

//        await dbContext.SaveChangesAsync();

//        var service = CreateService(dbContext);

//        // Act
//        var result = await service.GetDashboardAsync(workerId);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(5, result.LatestRequests.Count);

//        // The newest request should be first.
//        Assert.Equal("Reason 6", result.LatestRequests[0].CheckPurpose);
//        Assert.Equal("Reason 2", result.LatestRequests[4].CheckPurpose);
//    }
//}
