using Microsoft.AspNetCore.SignalR;
using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.DTOs;
using wdb_backend.Models;
using wdb_backend.Notification;

namespace wdb_backend.Tests;

// Unit tests for NotificationClientsHandler. The handler persists the
// inbound NotificationEvent then pushes a formatted message to the worker's
// SignalR group. IClientProxy.SendAsync is an extension method, so the
// underlying SendCoreAsync is what gets verified by Moq.
public class NotificationClientsHandlerTests
{
    [Fact]
    public async Task Handle_PersistsNotification_AndPushesFormattedMessage_ToWorkerGroup()
    {
        var mockHubContext = new Mock<IHubContext<NotificationsHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockProxy = new Mock<IClientProxy>();
        mockHubContext.SetupGet(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockProxy.Object);

        var mockRepo = new Mock<INotificationRepository>();
        var evt = new NotificationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.Access,
            new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc));
        var format = new NotificationFormat("Acme", "Luca", "Access", "Phone", evt.CreateAt);
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Models.Notification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.FormatNotification(evt, It.IsAny<CancellationToken>())).ReturnsAsync(format);

        var handler = new NotificationClientsHandler(mockHubContext.Object, mockRepo.Object);
        await handler.Handle(evt, default);

        // Persisted row reflects the event fields and starts unread.
        mockRepo.Verify(r => r.AddAsync(
            It.Is<Models.Notification>(n =>
                n.EmployerId == evt.EmployerId &&
                n.WorkerId == evt.WorkerId &&
                n.WorkerInfoId == evt.WorkerInfoId &&
                n.Type == "Access" &&
                n.IsRead == false &&
                n.CreateAt == evt.CreateAt),
            It.IsAny<CancellationToken>()), Times.Once);

        // Group key is the worker's GUID as string.
        mockClients.Verify(c => c.Group(evt.WorkerId.ToString()), Times.Once);

        // Message body assembled from the formatted DTO.
        var expectedMessage = $"Acme accessed your Phone at {evt.CreateAt}";
        mockProxy.Verify(p => p.SendCoreAsync(
            "NotificationInfo",
            It.Is<object?[]>(args => args.Length == 1 && (string)args[0]! == expectedMessage),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LowercasesNotificationTypeInMessage()
    {
        var mockHubContext = new Mock<IHubContext<NotificationsHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockProxy = new Mock<IClientProxy>();
        mockHubContext.SetupGet(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockProxy.Object);

        var mockRepo = new Mock<INotificationRepository>();
        var evt = new NotificationEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            NotificationType.Request,
            new DateTime(2026, 5, 18, 11, 0, 0, DateTimeKind.Utc));
        var format = new NotificationFormat("Acme", "Luca", "Request", "Phone", evt.CreateAt);
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Models.Notification>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.FormatNotification(evt, It.IsAny<CancellationToken>())).ReturnsAsync(format);

        var handler = new NotificationClientsHandler(mockHubContext.Object, mockRepo.Object);
        await handler.Handle(evt, default);

        // "Request" -> "requested" via the ToLower() + literal "ed" suffix in the handler.
        var expectedMessage = $"Acme requested your Phone at {evt.CreateAt}";
        mockProxy.Verify(p => p.SendCoreAsync(
            "NotificationInfo",
            It.Is<object?[]>(args => args.Length == 1 && (string)args[0]! == expectedMessage),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
