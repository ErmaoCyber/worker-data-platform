//using Microsoft.EntityFrameworkCore;
//using wdb_backend.Common;
//using wdb_backend.Data;
//using wdb_backend.Models;
//using wdb_backend.Services;

//namespace wdb_backend.Tests;

//public class EmployerSentRequestServiceTests
//{
//    private static AppDbContext CreateContext()
//    {
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(Guid.NewGuid().ToString())
//            .Options;

//        return new AppDbContext(options);
//    }

//    [Fact]
//    public async Task GetSentRequestsAsync_ReturnsRequestsForEmployer()
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
//            Status = PermissionStatus.Pending,
//            LastUpdatedAt = DateTime.UtcNow,
//            ExpiryDate = null
//        });

//        await context.SaveChangesAsync();

//        var service = new EmployerSentRequestServiceImpl(context);

//        // Act
//        var result = await service.GetSentRequestsAsync(employerId);

//        // Assert
//        Assert.Single(result);

//        var request = result[0];

//        Assert.Equal(requestId, request.RequestId);
//        Assert.Equal(workerId, request.WorkerId);
//        Assert.Equal("Alan Brown", request.WorkerName);
//        Assert.Equal("worker_001@test.com", request.WorkerEmail);
//        Assert.Equal("Site onboarding check", request.Reason);
//        Assert.Equal("Pending", request.Status);
//        Assert.Single(request.RequestedDataTypes);
//        Assert.Equal("PPE Requirements", request.RequestedDataTypes[0]);
//    }

//    [Fact]
//    public async Task GetSentRequestsAsync_OnlyReturnsRequestsForCurrentEmployer()
//    {
//        // Arrange
//        await using var context = CreateContext();

//        var currentEmployerId = Guid.NewGuid();
//        var otherEmployerId = Guid.NewGuid();
//        var workerId = Guid.NewGuid();

//        var currentRequestId = Guid.NewGuid();
//        var otherRequestId = Guid.NewGuid();

//        var infoId = Guid.NewGuid();

//        context.Employers.AddRange(
//            new Employer
//            {
//                Id = currentEmployerId,
//                Name = "BuildSafe Ltd",
//                Email = "buildsafe@test.com",
//                Password = "password",
//                Verified = true
//            },
//            new Employer
//            {
//                Id = otherEmployerId,
//                Name = "Other Company",
//                Email = "other@test.com",
//                Password = "password",
//                Verified = true
//            }
//        );

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
//            Desc = "country",
//            Value = "New Zealand"
//        });

//        context.Requests.AddRange(
//            new Request
//            {
//                Id = currentRequestId,
//                EmployerId = currentEmployerId,
//                WorkerId = workerId,
//                Reason = "Current employer request",
//                CreatedAt = DateTime.UtcNow.AddDays(-1)
//            },
//            new Request
//            {
//                Id = otherRequestId,
//                EmployerId = otherEmployerId,
//                WorkerId = workerId,
//                Reason = "Other employer request",
//                CreatedAt = DateTime.UtcNow.AddDays(-1)
//            }
//        );

//        context.Permissions.AddRange(
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = currentRequestId,
//                InfoId = infoId,
//                Status = PermissionStatus.Pending,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = null
//            },
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = otherRequestId,
//                InfoId = infoId,
//                Status = PermissionStatus.Pending,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = null
//            }
//        );

//        await context.SaveChangesAsync();

//        var service = new EmployerSentRequestServiceImpl(context);

//        // Act
//        var result = await service.GetSentRequestsAsync(currentEmployerId);

//        // Assert
//        Assert.Single(result);
//        Assert.Equal(currentRequestId, result[0].RequestId);
//        Assert.Equal("Current employer request", result[0].Reason);
//    }

//    [Fact]
//    public async Task GetSentRequestsAsync_ReturnsPartialStatus_WhenPermissionStatusesAreMixed()
//    {
//        // Arrange
//        await using var context = CreateContext();

//        var employerId = Guid.NewGuid();
//        var workerId = Guid.NewGuid();
//        var requestId = Guid.NewGuid();

//        var countryInfoId = Guid.NewGuid();
//        var genderInfoId = Guid.NewGuid();

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
//                Id = countryInfoId,
//                WorkerId = workerId,
//                Desc = "country",
//                Value = "New Zealand"
//            },
//            new WorkerInfo
//            {
//                Id = genderInfoId,
//                WorkerId = workerId,
//                Desc = "gender",
//                Value = "Male"
//            }
//        );

//        context.Requests.Add(new Request
//        {
//            Id = requestId,
//            EmployerId = employerId,
//            WorkerId = workerId,
//            Reason = "Mixed status test",
//            CreatedAt = DateTime.UtcNow.AddDays(-1)
//        });

//        context.Permissions.AddRange(
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = requestId,
//                InfoId = countryInfoId,
//                Status = PermissionStatus.Approved,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = DateTime.UtcNow.AddDays(7)
//            },
//            new Permission
//            {
//                Id = Guid.NewGuid(),
//                WorkerId = workerId,
//                RequestId = requestId,
//                InfoId = genderInfoId,
//                Status = PermissionStatus.Pending,
//                LastUpdatedAt = DateTime.UtcNow,
//                ExpiryDate = null
//            }
//        );

//        await context.SaveChangesAsync();

//        var service = new EmployerSentRequestServiceImpl(context);

//        // Act
//        var result = await service.GetSentRequestsAsync(employerId);

//        // Assert
//        Assert.Single(result);
//        Assert.Equal("Partial", result[0].Status);
//        Assert.Contains("country", result[0].RequestedDataTypes);
//        Assert.Contains("gender", result[0].RequestedDataTypes);
//    }

//    [Fact]
//    public async Task GetSentRequestsAsync_ThrowsUnauthorizedAccessException_WhenEmployerDoesNotExist()
//    {
//        // Arrange
//        await using var context = CreateContext();

//        var service = new EmployerSentRequestServiceImpl(context);
//        var missingEmployerId = Guid.NewGuid();

//        // Act & Assert
//        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
//            service.GetSentRequestsAsync(missingEmployerId));
//    }
//}
