using Moq;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;
using wdb_backend.Models;
using wdb_backend.Services;

namespace wdb_backend.Tests;

// Unit tests for NotificationServiceImpl. Mocks INotificationRepository so
// each test only validates service-layer decisions (delegation, branching).
public class NotificationServiceImplTests
{
    private readonly Mock<INotificationRepository> _mockRepo = new();

    // --- UpdateStatus ---

    [Fact]
    public async Task UpdateStatus_ReturnsTrue_AndInvokesRepo_WhenNotificationExists()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Models.Notification { Id = id, Type = "Access" });
        _mockRepo.Setup(r => r.UpdateStatusAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new NotificationServiceImpl(_mockRepo.Object);
        var ok = await service.UpdateStatus(id, default);

        Assert.True(ok);
        _mockRepo.Verify(r => r.UpdateStatusAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsFalse_AndSkipsRepo_WhenNotificationMissing()
    {
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.Notification?)null);

        var service = new NotificationServiceImpl(_mockRepo.Object);
        var ok = await service.UpdateStatus(id, default);

        Assert.False(ok);
        _mockRepo.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- Plain delegation queries ---

    [Fact]
    public async Task GetAllAsync_DelegatesToRepo()
    {
        var workerId = Guid.NewGuid();
        var stub = new List<Models.Notification> { new() { Id = Guid.NewGuid(), Type = "Access" } };
        _mockRepo.Setup(r => r.GetAllByWorkerIdAsync(workerId, It.IsAny<CancellationToken>())).ReturnsAsync(stub);

        var service = new NotificationServiceImpl(_mockRepo.Object);
        var result = await service.GetAllAsync(workerId, default);

        Assert.Same(stub, result);
    }

    [Fact]
    public async Task GetUnreadAsync_DelegatesToRepo()
    {
        var workerId = Guid.NewGuid();
        var stub = new List<Models.Notification>();
        _mockRepo.Setup(r => r.GetAllUnreadByWorkerIdAsync(workerId, It.IsAny<CancellationToken>())).ReturnsAsync(stub);

        var service = new NotificationServiceImpl(_mockRepo.Object);
        var result = await service.GetUnreadAsync(workerId, default);

        Assert.Same(stub, result);
    }

    [Fact]
    public async Task GetReadAsync_DelegatesToRepo()
    {
        var workerId = Guid.NewGuid();
        var stub = new List<Models.Notification>();
        _mockRepo.Setup(r => r.GetAllReadByWorkerIdAsync(workerId, It.IsAny<CancellationToken>())).ReturnsAsync(stub);

        var service = new NotificationServiceImpl(_mockRepo.Object);
        var result = await service.GetReadAsync(workerId, default);

        Assert.Same(stub, result);
    }

    // --- NotificationFormat (pipeline mapping) ---

    [Fact]
    public async Task NotificationFormat_CallsFormatPipelineForEachInput_AndPreservesOrder()
    {
        var n1 = new Models.Notification { Id = Guid.NewGuid(), Type = "Access" };
        var n2 = new Models.Notification { Id = Guid.NewGuid(), Type = "Request" };
        var c1 = new NotificationFormatComponent(n1.Id, "E1", "W1", "Access", "Phone", DateTime.UtcNow);
        var c2 = new NotificationFormatComponent(n2.Id, "E2", "W2", "Request", "Email", DateTime.UtcNow);
        _mockRepo.Setup(r => r.FormatNotificationPipeline(n1, It.IsAny<CancellationToken>())).ReturnsAsync(c1);
        _mockRepo.Setup(r => r.FormatNotificationPipeline(n2, It.IsAny<CancellationToken>())).ReturnsAsync(c2);

        var service = new NotificationServiceImpl(_mockRepo.Object);
        var result = await service.NotificationFormat(new List<Models.Notification> { n1, n2 }, default);

        Assert.Equal(2, result.Count);
        Assert.Equal(c1, result[0]);
        Assert.Equal(c2, result[1]);
        _mockRepo.Verify(r => r.FormatNotificationPipeline(It.IsAny<Models.Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // --- GetFormattedAsync (single-query path) ---

    [Fact]
    public async Task GetFormattedAsync_DelegatesWithIsReadFlag()
    {
        var workerId = Guid.NewGuid();
        var stub = new List<NotificationFormatComponent>();
        _mockRepo.Setup(r => r.GetFormattedNotificationsAsync(workerId, false, It.IsAny<CancellationToken>())).ReturnsAsync(stub);

        var service = new NotificationServiceImpl(_mockRepo.Object);
        var result = await service.GetFormattedAsync(workerId, false, default);

        Assert.Same(stub, result);
        _mockRepo.Verify(r => r.GetFormattedNotificationsAsync(workerId, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
