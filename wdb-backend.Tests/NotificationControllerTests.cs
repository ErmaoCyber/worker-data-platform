//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using wdb_backend.Abstractions;
//using wdb_backend.Common;
//using wdb_backend.Controllers;
//using wdb_backend.DTOs;

//namespace wdb_backend.Tests;

//// Unit tests for NotificationController. Mocks IMediator (for POST endpoints)
//// and INotificationService (for PATCH/GET endpoints) and asserts both the
//// HTTP shape and the command/flag forwarded downstream.
//public class NotificationControllerTests
//{
//    private readonly Mock<IMediator> _mockMediator = new();
//    private readonly Mock<INotificationService> _mockService = new();
//    private readonly NotificationController _controller;

//    public NotificationControllerTests()
//    {
//        _controller = new NotificationController(_mockMediator.Object, _mockService.Object);
//    }

//    // --- POST /access ---

//    [Fact]
//    public async Task AccessInfo_SendsAccessCommand_AndReturnsOk()
//    {
//        var info = new NotificationInfo(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

//        var result = await _controller.AccessInfo(info, default);

//        Assert.IsType<OkObjectResult>(result);
//        _mockMediator.Verify(m => m.Send(
//            It.Is<NotificationCommand>(c =>
//                c.EmployerId == info.EmployerId &&
//                c.WorkerId == info.WorkerId &&
//                c.WorkerInfoId == info.WorkerInfoId &&
//                c.Type == NotificationType.Access),
//            It.IsAny<CancellationToken>()), Times.Once);
//    }

//    // --- POST /request ---

//    [Fact]
//    public async Task RequestInfo_SendsRequestCommand_AndReturnsOk()
//    {
//        var info = new NotificationInfo(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

//        var result = await _controller.RequestInfo(info, default);

//        Assert.IsType<OkObjectResult>(result);
//        _mockMediator.Verify(m => m.Send(
//            It.Is<NotificationCommand>(c =>
//                c.EmployerId == info.EmployerId &&
//                c.WorkerId == info.WorkerId &&
//                c.WorkerInfoId == info.WorkerInfoId &&
//                c.Type == NotificationType.Request),
//            It.IsAny<CancellationToken>()), Times.Once);
//    }

//    // --- PATCH /{notificationId} ---

//    [Fact]
//    public async Task CheckNotification_ReturnsOk_WhenServiceReportsSuccess()
//    {
//        var id = Guid.NewGuid();
//        _mockService.Setup(s => s.UpdateStatus(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

//        var result = await _controller.CheckNotification(id, default);

//        Assert.IsType<OkObjectResult>(result);
//    }

//    [Fact]
//    public async Task CheckNotification_ReturnsNotFound_WhenServiceReportsMissing()
//    {
//        var id = Guid.NewGuid();
//        _mockService.Setup(s => s.UpdateStatus(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

//        var result = await _controller.CheckNotification(id, default);

//        Assert.IsType<NotFoundObjectResult>(result);
//    }

//    // --- GET /all|/unread|/read/{workerId} (single-query path) ---

//    [Fact]
//    public async Task GetAll_ReturnsOk_AndPassesNullIsReadFlag()
//    {
//        var workerId = Guid.NewGuid();
//        var stub = new List<NotificationFormatComponent>
//        {
//            new(Guid.NewGuid(), "Acme", null, "Access", "Phone", DateTime.UtcNow)
//        };
//        _mockService.Setup(s => s.GetFormattedAsync(workerId, null, It.IsAny<CancellationToken>())).ReturnsAsync(stub);

//        var result = await _controller.GetAll(workerId, default);

//        var ok = Assert.IsType<OkObjectResult>(result.Result);
//        Assert.NotNull(ok.Value);
//        _mockService.Verify(s => s.GetFormattedAsync(workerId, null, It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task GetUnread_ReturnsOk_AndPassesFalseIsReadFlag()
//    {
//        var workerId = Guid.NewGuid();
//        _mockService.Setup(s => s.GetFormattedAsync(workerId, false, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new List<NotificationFormatComponent>());

//        var result = await _controller.GetUnread(workerId, default);

//        Assert.IsType<OkObjectResult>(result);
//        _mockService.Verify(s => s.GetFormattedAsync(workerId, false, It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task GetRead_ReturnsOk_AndPassesTrueIsReadFlag()
//    {
//        var workerId = Guid.NewGuid();
//        _mockService.Setup(s => s.GetFormattedAsync(workerId, true, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new List<NotificationFormatComponent>());

//        var result = await _controller.GetRead(workerId, default);

//        Assert.IsType<OkObjectResult>(result);
//        _mockService.Verify(s => s.GetFormattedAsync(workerId, true, It.IsAny<CancellationToken>()), Times.Once);
//    }
//}
