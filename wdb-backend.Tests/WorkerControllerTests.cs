//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using System.Security.Claims;
//using wdb_backend.Abstractions;
//using wdb_backend.Common;
//using wdb_backend.Controllers;
//using wdb_backend.DTOs;
//using wdb_backend.Models;

//namespace wdb_backend.Tests;

//public class WorkerControllerTests
//{
//    private readonly Mock<IPermissionService> _mockPermission;
//    private readonly Mock<IRequestService> _mockRequest;
//    private readonly Mock<IWorkerInfoService> _mockWorkerInfo;
//    private readonly Mock<IEmployerService> _mockEmployer;
//    private readonly Mock<IActiveAccessService> _mockActiveAccessService;
//    private readonly WorkerController _controller;

//    public WorkerControllerTests()
//    {
//        _mockPermission = new Mock<IPermissionService>();
//        _mockRequest = new Mock<IRequestService>();
//        _mockWorkerInfo = new Mock<IWorkerInfoService>();
//        _mockEmployer = new Mock<IEmployerService>();
//        _mockActiveAccessService = new Mock<IActiveAccessService>();

//        _controller = new WorkerController(
//            _mockPermission.Object,
//            _mockRequest.Object,
//            _mockWorkerInfo.Object,
//            _mockEmployer.Object,
//            _mockActiveAccessService.Object
//        );
//    }

//    // --- GetPermissions ---

//    [Fact]
//    public async Task GetPermissions_ReturnsOk_WhenPermissionsExist()
//    {
//        var workerId = Guid.NewGuid();

//        var permissions = new List<Permission>
//        {
//            new Permission
//            {
//                Status = PermissionStatus.Pending
//            }
//        };

//        _mockPermission
//            .Setup(service => service.GetAllByWorkerIdAsync(
//                workerId,
//                0,
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(permissions);

//        var result = await _controller.GetPermissions(workerId);

//        var ok = Assert.IsType<OkObjectResult>(result.Result);
//        Assert.Equal(permissions, ok.Value);
//    }

//    [Fact]
//    public async Task GetPermissions_ReturnsNotFound_WhenNull()
//    {
//        var workerId = Guid.NewGuid();

//        _mockPermission
//            .Setup(service => service.GetAllByWorkerIdAsync(
//                workerId,
//                0,
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync((List<Permission>?)null);

//        var result = await _controller.GetPermissions(workerId);

//        Assert.IsType<NotFoundObjectResult>(result.Result);
//    }

//    // --- GetAllWorkerInfo ---

//    [Fact]
//    public async Task GetAllWorkerInfo_ReturnsOk_WhenInfoExists()
//    {
//        var workerId = Guid.NewGuid();

//        var info = new List<WorkerInfo>
//        {
//            new WorkerInfo
//            {
//                Desc = "Job Title",
//                Value = "Engineer"
//            }
//        };

//        _mockWorkerInfo
//            .Setup(service => service.GetAllAsync(
//                workerId,
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(info);

//        var result = await _controller.GetAllWorkerInfo(workerId);

//        var ok = Assert.IsType<OkObjectResult>(result.Result);
//        Assert.Equal(info, ok.Value);
//    }

//    [Fact]
//    public async Task GetAllWorkerInfo_ReturnsNotFound_WhenNull()
//    {
//        var workerId = Guid.NewGuid();

//        _mockWorkerInfo
//            .Setup(service => service.GetAllAsync(
//                workerId,
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync((List<WorkerInfo>?)null);

//        var result = await _controller.GetAllWorkerInfo(workerId);

//        Assert.IsType<NotFoundObjectResult>(result.Result);
//    }

//    // --- GetRows ---

//    [Fact]
//    public async Task GetRows_ReturnsUnauthorized_WhenNoIdentityClaim()
//    {
//        _controller.ControllerContext = new ControllerContext
//        {
//            HttpContext = new DefaultHttpContext()
//        };

//        var result = await _controller.GetRows();

//        Assert.IsType<UnauthorizedResult>(result);
//    }

//    [Fact]
//    public async Task GetRows_ReturnsOk_WhenValidUser()
//    {
//        var workerId = Guid.NewGuid();

//        SetUserClaim(_controller, workerId);

//        _mockRequest
//            .Setup(service => service.GetAllByWorkerIdAsync(
//                workerId,
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new List<Request>());

//        _mockWorkerInfo
//            .Setup(service => service.GetAllAsync(
//                workerId,
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new List<WorkerInfo>());

//        _mockPermission
//            .Setup(service => service.GetAllByWorkerIdAsync(
//                workerId,
//                0,
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new List<Permission>());

//        _mockEmployer
//            .Setup(service => service.GetDistinctEmployers(
//                It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new List<Employer>());

//        var result = await _controller.GetRows();

//        Assert.IsType<OkObjectResult>(result);
//    }

//    // --- GetActiveAccess ---

//    [Fact]
//    public async Task GetActiveAccess_ReturnsOk_WithEmptyList_WhenNoActiveAccess()
//    {
//        var workerId = Guid.NewGuid();

//        _mockActiveAccessService
//            .Setup(service => service.GetActiveAccessAsync(
//                workerId,
//                null,
//                null))
//            .ReturnsAsync(new List<ActiveAccessDto>());

//        var result = await _controller.GetActiveAccess(workerId);

//        var ok = Assert.IsType<OkObjectResult>(result.Result);
//        var value = Assert.IsType<List<ActiveAccessDto>>(ok.Value);

//        Assert.Empty(value);
//    }

//    [Fact]
//    public async Task GetActiveAccess_CallsActiveAccessService_WithFilters()
//    {
//        var workerId = Guid.NewGuid();
//        var company = "first";
//        var dataType = "country";

//        var expectedResult = new List<ActiveAccessDto>
//        {
//            new ActiveAccessDto
//            {
//                RequestId = Guid.NewGuid(),
//                CompanyName = "firstste",
//                GrantedAt = DateTime.UtcNow,
//                Reason = "Employment onboarding",
//                WorkerInfo = new List<ActiveAccessInfoDto>
//                {
//                    new ActiveAccessInfoDto
//                    {
//                        PermissionId = Guid.NewGuid(),
//                        DataType = "country"
//                    }
//                }
//            }
//        };

//        _mockActiveAccessService
//            .Setup(service => service.GetActiveAccessAsync(
//                workerId,
//                company,
//                dataType))
//            .ReturnsAsync(expectedResult);

//        var result = await _controller.GetActiveAccess(workerId, company, dataType);

//        var ok = Assert.IsType<OkObjectResult>(result.Result);
//        var value = Assert.IsType<List<ActiveAccessDto>>(ok.Value);

//        Assert.Single(value);
//        Assert.Equal("firstste", value[0].CompanyName);
//        Assert.Equal("country", value[0].WorkerInfo[0].DataType);

//        _mockActiveAccessService.Verify(
//            service => service.GetActiveAccessAsync(workerId, company, dataType),
//            Times.Once);
//    }

//    // --- Helpers ---

//    private static void SetUserClaim(WorkerController controller, Guid workerId)
//    {
//        var claims = new List<Claim>
//        {
//            new Claim(ClaimTypes.NameIdentifier, workerId.ToString())
//        };

//        var identity = new ClaimsIdentity(claims, "test");
//        var principal = new ClaimsPrincipal(identity);

//        controller.ControllerContext = new ControllerContext
//        {
//            HttpContext = new DefaultHttpContext
//            {
//                User = principal
//            }
//        };
//    }
//}
