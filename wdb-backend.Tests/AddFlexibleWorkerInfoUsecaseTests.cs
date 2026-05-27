using Moq;
using wdb_backend.Abstractions;
using wdb_backend.Models;
using wdb_backend.Usecases;
using Xunit;

namespace wdb_backend.Tests;

public class AddFlexibleWorkerInfoUsecaseTests
{
    private readonly Mock<IWorkerService> _mockWorkerService;
    private readonly Mock<IWorkerInfoService> _mockWorkerInfoService;
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly AddFlexibleWorkerInfoUsecaseImpl _usecase;

    public AddFlexibleWorkerInfoUsecaseTests()
    {
        _mockWorkerService = new Mock<IWorkerService>();
        _mockWorkerInfoService = new Mock<IWorkerInfoService>();
        _mockRequestService = new Mock<IRequestService>();

        _usecase = new AddFlexibleWorkerInfoUsecaseImpl(
            _mockWorkerService.Object,
            _mockWorkerInfoService.Object,
            _mockRequestService.Object
        );
    }

    // Test 1: 正常流程，worker 存在，正确创建 WorkerInfo 和 Request
    [Fact]
    public async Task ExecuteAsync_ValidInput_CreatesWorkerInfoAndRequest()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var employerId = Guid.NewGuid();
        var workerEmail = "worker@test.com";
        var category = "PersonaInformation";
        var desc = "LinkedIn Profile";
        var reason = "Background check";

        var worker = new Worker { Id = workerId, Email = workerEmail };

        _mockWorkerService
            .Setup(s => s.GetByEmailAsync(workerEmail, default))
            .ReturnsAsync(worker);

        _mockWorkerInfoService
            .Setup(s => s.CreateAsync(workerId, It.IsAny<WorkerInfo>(), default))
            .ReturnsAsync(new WorkerInfo { WorkerId = workerId, Desc = desc, Value = "" });

        _mockRequestService
            .Setup(s => s.CreateAsync(employerId, workerId, reason, default))
            .ReturnsAsync(new Request { EmployerId = employerId, WorkerId = workerId, Reason = reason });

        // Act
        await _usecase.ExecuteAsync(workerEmail, category, desc, reason, employerId);

        // Assert
        _mockWorkerInfoService.Verify(
            s => s.CreateAsync(workerId, It.Is<WorkerInfo>(w =>
                w.Desc == desc &&
                w.Value == "" &&
                w.WorkerId == workerId
            ), default),
            Times.Once
        );

        _mockRequestService.Verify(
            s => s.CreateAsync(employerId, workerId, reason, default),
            Times.Once
        );
    }

    // Test 2: worker 不存在，应该抛出异常
    [Fact]
    public async Task ExecuteAsync_WorkerNotFound_ThrowsException()
    {
        // Arrange
        var workerEmail = "notfound@test.com";

        _mockWorkerService
            .Setup(s => s.GetByEmailAsync(workerEmail, default))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _usecase.ExecuteAsync(workerEmail, "PersonaInformation", "desc", "reason", Guid.NewGuid())
        );

        // WorkerInfo 和 Request 都不应该被创建
        _mockWorkerInfoService.Verify(
            s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<WorkerInfo>(), default),
            Times.Never
        );

        _mockRequestService.Verify(
            s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), default),
            Times.Never
        );
    }

    // Test 3: 无效的 category，应该抛出异常
    [Fact]
    public async Task ExecuteAsync_InvalidCategory_ThrowsException()
    {
        // Arrange
        var workerId = Guid.NewGuid();
        var worker = new Worker { Id = workerId, Email = "worker@test.com" };

        _mockWorkerService
            .Setup(s => s.GetByEmailAsync("worker@test.com", default))
            .ReturnsAsync(worker);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _usecase.ExecuteAsync("worker@test.com", "InvalidCategory", "desc", "reason", Guid.NewGuid())
        );
    }
}