//using MediatR;
//using Moq;
//using wdb_backend.Common;
//using wdb_backend.DTOs;
//using wdb_backend.Notification;

//namespace wdb_backend.Tests;

//// Unit tests for NotificationCommandHandler. The handler simply translates
//// an inbound command into a published NotificationEvent stamped with UtcNow.
//public class NotificationCommandHandlerTests
//{
//    [Fact]
//    public async Task Handle_PublishesNotificationEvent_WithSameFields()
//    {
//        var mockMediator = new Mock<IMediator>();
//        var handler = new NotificationCommandHandler(mockMediator.Object);
//        var cmd = new NotificationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), NotificationType.Access);

//        await handler.Handle(cmd, default);

//        mockMediator.Verify(m => m.Publish(
//            It.Is<NotificationEvent>(e =>
//                e.EmployerId == cmd.EmployerId &&
//                e.WorkerId == cmd.WorkerId &&
//                e.WorkerInfoId == cmd.WorkerInfoId &&
//                e.Type == cmd.Type),
//            It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_StampsCreateAtToCurrentUtcTime()
//    {
//        var mockMediator = new Mock<IMediator>();
//        var handler = new NotificationCommandHandler(mockMediator.Object);
//        var cmd = new NotificationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), NotificationType.Request);

//        // Bracket the call with UtcNow to assert the timestamp falls inside the window.
//        var before = DateTime.UtcNow;
//        await handler.Handle(cmd, default);
//        var after = DateTime.UtcNow;

//        mockMediator.Verify(m => m.Publish(
//            It.Is<NotificationEvent>(e => e.CreateAt >= before && e.CreateAt <= after),
//            It.IsAny<CancellationToken>()), Times.Once);
//    }
//}
