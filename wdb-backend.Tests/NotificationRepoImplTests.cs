//using Microsoft.EntityFrameworkCore;
//using wdb_backend.Common;
//using wdb_backend.Data;
//using wdb_backend.DTOs;
//using wdb_backend.Models;
//using wdb_backend.Services;

//namespace wdb_backend.Tests;

//// Unit tests for NotificationRepoImpl. Uses EF Core InMemory provider with
//// a unique database name per test method to avoid cross-test state leakage.
//public class NotificationRepoImplTests
//{
//    private static AppDbContext CreateContext(string dbName)
//    {
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(databaseName: dbName)
//            .Options;
//        return new AppDbContext(options);
//    }

//    // Helper to build a Notification with sensible defaults; only override
//    // fields the test cares about. Fully qualified to avoid the namespace
//    // collision with wdb_backend.Notification.
//    private static Models.Notification MakeNotification(
//        Guid? id = null,
//        Guid? employerId = null,
//        Guid? workerId = null,
//        Guid? workerInfoId = null,
//        string type = "Access",
//        bool isRead = false,
//        DateTime? createAt = null) => new()
//    {
//        Id = id ?? Guid.NewGuid(),
//        EmployerId = employerId ?? Guid.NewGuid(),
//        WorkerId = workerId ?? Guid.NewGuid(),
//        WorkerInfoId = workerInfoId ?? Guid.NewGuid(),
//        Type = type,
//        IsRead = isRead,
//        CreateAt = createAt ?? DateTime.UtcNow
//    };

//    // --- AddAsync ---

//    [Fact]
//    public async Task AddAsync_PersistsNotification()
//    {
//        using var ctx = CreateContext(nameof(AddAsync_PersistsNotification));
//        var repo = new NotificationRepoImpl(ctx);
//        var notification = MakeNotification(type: NotificationType.Access.ToString());

//        await repo.AddAsync(notification);

//        Assert.Equal(1, ctx.Notifications.Count());
//        Assert.Equal(notification.Id, ctx.Notifications.First().Id);
//    }

//    // --- UpdateStatusAsync ---

//    [Fact]
//    public async Task UpdateStatusAsync_SetsIsReadTrue_WhenExists()
//    {
//        using var ctx = CreateContext(nameof(UpdateStatusAsync_SetsIsReadTrue_WhenExists));
//        var id = Guid.NewGuid();
//        ctx.Notifications.Add(MakeNotification(id: id, isRead: false));
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        await repo.UpdateStatusAsync(id);

//        Assert.True(ctx.Notifications.First(n => n.Id == id).IsRead);
//    }

//    [Fact]
//    public async Task UpdateStatusAsync_DoesNothing_WhenNotFound()
//    {
//        using var ctx = CreateContext(nameof(UpdateStatusAsync_DoesNothing_WhenNotFound));
//        var repo = new NotificationRepoImpl(ctx);

//        // Should silently no-op; controller layer translates absence into 404.
//        await repo.UpdateStatusAsync(Guid.NewGuid());

//        Assert.Empty(ctx.Notifications);
//    }

//    // --- GetAll / GetAllUnread / GetAllRead by workerId ---

//    [Fact]
//    public async Task GetAllByWorkerIdAsync_ReturnsOnlyMatchingWorker()
//    {
//        using var ctx = CreateContext(nameof(GetAllByWorkerIdAsync_ReturnsOnlyMatchingWorker));
//        var workerA = Guid.NewGuid();
//        var workerB = Guid.NewGuid();
//        ctx.Notifications.AddRange(
//            MakeNotification(workerId: workerA),
//            MakeNotification(workerId: workerA, isRead: true),
//            MakeNotification(workerId: workerB));
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var result = await repo.GetAllByWorkerIdAsync(workerA);

//        Assert.Equal(2, result.Count);
//        Assert.All(result, n => Assert.Equal(workerA, n.WorkerId));
//    }

//    [Fact]
//    public async Task GetAllUnreadByWorkerIdAsync_ReturnsOnlyUnreadForWorker()
//    {
//        using var ctx = CreateContext(nameof(GetAllUnreadByWorkerIdAsync_ReturnsOnlyUnreadForWorker));
//        var workerId = Guid.NewGuid();
//        ctx.Notifications.AddRange(
//            MakeNotification(workerId: workerId, isRead: false),
//            MakeNotification(workerId: workerId, isRead: true),
//            MakeNotification(isRead: false)); // belongs to a different worker
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var result = await repo.GetAllUnreadByWorkerIdAsync(workerId);

//        Assert.Single(result);
//        Assert.False(result[0].IsRead);
//        Assert.Equal(workerId, result[0].WorkerId);
//    }

//    [Fact]
//    public async Task GetAllReadByWorkerIdAsync_ReturnsOnlyReadForWorker()
//    {
//        using var ctx = CreateContext(nameof(GetAllReadByWorkerIdAsync_ReturnsOnlyReadForWorker));
//        var workerId = Guid.NewGuid();
//        ctx.Notifications.AddRange(
//            MakeNotification(workerId: workerId, isRead: true),
//            MakeNotification(workerId: workerId, isRead: false),
//            MakeNotification(isRead: true)); // belongs to a different worker
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var result = await repo.GetAllReadByWorkerIdAsync(workerId);

//        Assert.Single(result);
//        Assert.True(result[0].IsRead);
//        Assert.Equal(workerId, result[0].WorkerId);
//    }

//    // --- GetByIdAsync ---

//    [Fact]
//    public async Task GetByIdAsync_ReturnsNotification_WhenExists()
//    {
//        using var ctx = CreateContext(nameof(GetByIdAsync_ReturnsNotification_WhenExists));
//        var id = Guid.NewGuid();
//        ctx.Notifications.Add(MakeNotification(id: id));
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var result = await repo.GetByIdAsync(id, default);

//        Assert.NotNull(result);
//        Assert.Equal(id, result!.Id);
//    }

//    [Fact]
//    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
//    {
//        using var ctx = CreateContext(nameof(GetByIdAsync_ReturnsNull_WhenNotExists));
//        var repo = new NotificationRepoImpl(ctx);

//        var result = await repo.GetByIdAsync(Guid.NewGuid(), default);

//        Assert.Null(result);
//    }

//    // --- FormatNotification (from NotificationEvent) ---

//    [Fact]
//    public async Task FormatNotification_PopulatesNames_WhenReferencesExist()
//    {
//        using var ctx = CreateContext(nameof(FormatNotification_PopulatesNames_WhenReferencesExist));
//        var empId = Guid.NewGuid();
//        var workerId = Guid.NewGuid();
//        var infoId = Guid.NewGuid();
//        ctx.Employers.Add(new Employer { Id = empId, Name = "Acme Co", Email = "a@a.com", Password = "x" });
//        ctx.Workers.Add(new Worker { Id = workerId, Name = "Luca", Email = "l@l.com", Password = "x" });
//        ctx.WorkerInfos.Add(new WorkerInfo { Id = infoId, WorkerId = workerId, Desc = "Phone", Value = "12345" });
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var when = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);
//        var evt = new NotificationEvent(empId, workerId, infoId, NotificationType.Access, when);

//        var format = await repo.FormatNotification(evt, default);

//        Assert.Equal("Acme Co", format.EmployerName);
//        Assert.Equal("Luca", format.WorkerName);
//        Assert.Equal("Phone", format.WorkInfoDesc);
//        Assert.Equal("Access", format.NotificationType);
//        Assert.Equal(when, format.NotificationTime);
//    }

//    [Fact]
//    public async Task FormatNotification_LeavesNamesNull_WhenReferencesMissing()
//    {
//        using var ctx = CreateContext(nameof(FormatNotification_LeavesNamesNull_WhenReferencesMissing));
//        var repo = new NotificationRepoImpl(ctx);
//        var evt = new NotificationEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), NotificationType.Request, DateTime.UtcNow);

//        var format = await repo.FormatNotification(evt, default);

//        Assert.Null(format.EmployerName);
//        Assert.Null(format.WorkerName);
//        Assert.Null(format.WorkInfoDesc);
//        Assert.Equal("Request", format.NotificationType);
//    }

//    // --- FormatNotificationPipeline (from persisted Notification) ---

//    [Fact]
//    public async Task FormatNotificationPipeline_PopulatesAllFieldsFromNotification()
//    {
//        using var ctx = CreateContext(nameof(FormatNotificationPipeline_PopulatesAllFieldsFromNotification));
//        var empId = Guid.NewGuid();
//        var workerId = Guid.NewGuid();
//        var infoId = Guid.NewGuid();
//        ctx.Employers.Add(new Employer { Id = empId, Name = "Acme", Email = "e@e.com", Password = "x" });
//        ctx.Workers.Add(new Worker { Id = workerId, Name = "Luca", Email = "l@l.com", Password = "x" });
//        ctx.WorkerInfos.Add(new WorkerInfo { Id = infoId, WorkerId = workerId, Desc = "Email", Value = "z@z.com" });
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var notif = MakeNotification(employerId: empId, workerId: workerId, workerInfoId: infoId, type: "Request");

//        var comp = await repo.FormatNotificationPipeline(notif, default);

//        Assert.Equal(notif.Id, comp.Id);
//        Assert.Equal("Acme", comp.EmployerName);
//        Assert.Equal("Luca", comp.WorkerName);
//        Assert.Equal("Email", comp.WorkerInfoDesc);
//        Assert.Equal("Request", comp.NotificationType);
//    }

//    // --- GetFormattedNotificationsAsync (single JOIN, isRead = null | false | true) ---

//    [Fact]
//    public async Task GetFormattedNotificationsAsync_ReturnsAllForWorker_WhenIsReadNull()
//    {
//        using var ctx = CreateContext(nameof(GetFormattedNotificationsAsync_ReturnsAllForWorker_WhenIsReadNull));
//        var empId = Guid.NewGuid();
//        var workerA = Guid.NewGuid();
//        var workerB = Guid.NewGuid();
//        var infoId = Guid.NewGuid();
//        ctx.Employers.Add(new Employer { Id = empId, Name = "Acme", Email = "e@e.com", Password = "x" });
//        ctx.WorkerInfos.Add(new WorkerInfo { Id = infoId, WorkerId = workerA, Desc = "Phone", Value = "1" });
//        ctx.Notifications.AddRange(
//            MakeNotification(employerId: empId, workerId: workerA, workerInfoId: infoId, isRead: false),
//            MakeNotification(employerId: empId, workerId: workerA, workerInfoId: infoId, isRead: true),
//            MakeNotification(employerId: empId, workerId: workerB, workerInfoId: infoId, isRead: false));
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var result = await repo.GetFormattedNotificationsAsync(workerA, null, default);

//        Assert.Equal(2, result.Count);
//        Assert.All(result, r => Assert.Equal("Acme", r.EmployerName));
//        Assert.All(result, r => Assert.Equal("Phone", r.WorkerInfoDesc));
//    }

//    [Fact]
//    public async Task GetFormattedNotificationsAsync_ReturnsOnlyUnread_WhenIsReadFalse()
//    {
//        using var ctx = CreateContext(nameof(GetFormattedNotificationsAsync_ReturnsOnlyUnread_WhenIsReadFalse));
//        var workerId = Guid.NewGuid();
//        ctx.Notifications.AddRange(
//            MakeNotification(workerId: workerId, isRead: false),
//            MakeNotification(workerId: workerId, isRead: true));
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var result = await repo.GetFormattedNotificationsAsync(workerId, false, default);

//        Assert.Single(result);
//    }

//    [Fact]
//    public async Task GetFormattedNotificationsAsync_ReturnsOnlyRead_WhenIsReadTrue()
//    {
//        using var ctx = CreateContext(nameof(GetFormattedNotificationsAsync_ReturnsOnlyRead_WhenIsReadTrue));
//        var workerId = Guid.NewGuid();
//        ctx.Notifications.AddRange(
//            MakeNotification(workerId: workerId, isRead: false),
//            MakeNotification(workerId: workerId, isRead: true));
//        await ctx.SaveChangesAsync();

//        var repo = new NotificationRepoImpl(ctx);
//        var result = await repo.GetFormattedNotificationsAsync(workerId, true, default);

//        Assert.Single(result);
//    }
//}
