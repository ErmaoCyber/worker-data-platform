//using Microsoft.EntityFrameworkCore;
//using wdb_backend.Common;
//using wdb_backend.Data;
//using wdb_backend.Models;
//using wdb_backend.Services;

//namespace wdb_backend.Tests.Services;

//public class EmployerActiveAccessServiceTests
//{
//    private static AppDbContext CreateContext()
//    {
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(Guid.NewGuid().ToString())
//            .Options;

//        return new AppDbContext(options);
//    }

//    [Fact]
//    public async Task GetActiveAccessAsync_ReturnsApprovedActiveAccess()
//    {
//        // Arrange
//        await using var context = CreateContext();

//        var employerId = Guid.NewGuid();
//        var workerId = Guid.NewGuid();
//        var requestId = Guid.NewGuid();
//        var infoId = Guid.NewGuid();
//        var permissionId = Guid.NewGuid();

//        context.Employers.Add(new Employer
//        {
//            Id = employerId,
//            Name = "BuildSafe Ltd",
//            Email = "buildsafe@test.com",
//            Password = "password",
//            Verified = true
//        });

//        context.Workers.Add(new Worker
//        {
//            Id = workerId,
//            Name = "Alan Brown",
//            Email = "worker_001@test.com",
//            Password = "password",
//            Verified = true
//        });

//        context.WorkerInfos.Add(new WorkerInfo
//        {
//            Id = infoId,
//            WorkerId = workerId,
//            Desc = "PPE Requirements",
//            Value = "Safety boots"
//        });

//        context.Requests.Add(new Request
//        {
//            Id = requestId,
//            EmployerId = employerId,
//            WorkerId = workerId,
//            Reason = "Site onboarding check",
//            CreatedAt = DateTime.UtcNow.AddDays(-1)
//        });

//        context.Permissions.Add(new Permission
//        {
//            Id = permissionId,
//            WorkerId = workerId,
//            RequestId = requestId,
//            InfoId = infoId,
//            Status = PermissionStatus.Approved,
//            LastUpdatedAt = DateTime.UtcNow,
//            ExpiryDate = DateTime.UtcNow.AddDays(7)
//        });

//        await context.SaveChangesAsync();

//        var service = new EmployerActiveAccessServiceImpl(context);

//        // Act
//        var result = await service.GetActiveAccessAsync(employerId);

//        // Assert
//        Assert.Single(result);

//        var access = result[0];

//        Assert.Equal(requestId, access.RequestId);
//        Assert.Equal(workerId, access.WorkerId);
//        Assert.Equal("Alan Brown", access.WorkerName);
//        Assert.Equal("worker_001@test.com", access.WorkerEmail);
//        Assert.Equal("Site onboarding check", access.Reason);
//        Assert.Single(access.WorkerInfo);
//        Assert.Equal(permissionId, access.WorkerInfo[0].PermissionId);
//        Assert.Equal("PPE Requirements", access.WorkerInfo[0].DataType);
//        Assert.Equal("Safety boots", access.WorkerInfo[0].Value);
//    }

//    [Fact]
//    public async Task GetActiveAccessAsync_ExcludesPendingRejectedAndExpiredPermissions()
//    {
//        // Arrange
//        await using var context = CreateContext();

//        var employerId = Guid.NewGuid();
//        var workerId = Guid.NewGuid();
//        var requestId = Guid.NewGuid();

//        var approvedInfoId = Guid.NewGuid();
//        var pendingInfoId = Guid.NewGuid();
//        var rejectedInfoId = Guid.NewGuid();
//        var expiredInfoId = Guid.NewGuid();

//        context.Employers.Add(new Employer
//        {
//            Id = employerId,
//            Name = "BuildSafe Ltd",
//            Email = "buildsafe@test.com",
//            Password = "password",
//            Verified = true
//        });

//        context.Workers.Add(new Worker
//        {
//            Id = workerId,
//            Name = "Alan Brown",
//            Email = "worker_001@test.com",
//            Password = "password",
//            Verified = true
//        });

//        context.WorkerInfos.AddRange(
//            new WorkerInfo
//            {
//                Id = approvedInfoId,
//                WorkerId = workerId,
//                Desc = "country",
//                Value = "New Zealand"
//            },
//            new WorkerInfo
//            {
//                Id = pendingInfoId,
//                WorkerId = workerId,
//                Desc = "gender",
//                Value = "Male"
//            },
//            new WorkerInfo
//            {
//                Id = rejectedInfoId,
//                WorkerId = workerId,
//                Desc = "Emergency Contact",
//                Value = "Sarah Brown"
//            },
//            new WorkerInfo
//            {
//                Id = expiredInfoId,
//                WorkerId = workerId,
//                Desc = "PPE Requirements",
//                Value = "Safety boots"
//            }
//        );

//        context.Requests.Add(new Request
//        {
//            Id = requestId,
//            EmployerId = employerId,
//            WorkerId = workerId,
//            Reason = "Access filtering test",
//            CreatedAt = DateTime.UtcNow.AddDays(-1)
//        });

//        context.Permissions.AddRange(
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = requestId,
//                InfoId = approvedInfoId,
//                Status = PermissionStatus.Approved,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = DateTime.UtcNow.AddDays(7)
//            },
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = requestId,
//                InfoId = pendingInfoId,
//                Status = PermissionStatus.Pending,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = null
//            },
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = requestId,
//                InfoId = rejectedInfoId,
//                Status = PermissionStatus.Rejected,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = null
//            },
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = requestId,
//                InfoId = expiredInfoId,
//                Status = PermissionStatus.Approved,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = DateTime.UtcNow.AddDays(-1)
//            }
//        );

//        await context.SaveChangesAsync();

//        var service = new EmployerActiveAccessServiceImpl(context);

//        // Act
//        var result = await service.GetActiveAccessAsync(employerId);

//        // Assert
//        Assert.Single(result);
//        Assert.Single(result[0].WorkerInfo);
//        Assert.Equal("country", result[0].WorkerInfo[0].DataType);
//    }
//}
