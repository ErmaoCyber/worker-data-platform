//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using wdb_backend.Abstractions;
//using wdb_backend.Controllers;
//using wdb_backend.Models;
//using wdb_backend.DTOs;
//using System.Security.Claims;
//using System.IdentityModel.Tokens.Jwt;
//using Microsoft.AspNetCore.Http;
//using Org.BouncyCastle.Asn1.Misc;
//using System.ComponentModel;

//public class EmployerControllerTests
//{
//    // 共用的 mock，避免每个测试重复创建
//    private readonly Mock<ICreateDataAccessRequestUsecase> _mockCreateDataUsecase = new();
//    private readonly Mock<IFindWorkerInfosByEmailUsecase> _mockFindInfosUsecase = new();
//    private readonly Mock<IWorkerService> _mockWorkerService = new();
//    private readonly Mock<IAddFlexibleWorkerInfoUsecase> _mockAddFlexibleWorkerInfoUsecase = new();

//    private EmployerController CreateController(Guid employerId)
//    {
//        var controller = new EmployerController(
//            _mockCreateDataUsecase.Object,
//            _mockFindInfosUsecase.Object,
//            _mockWorkerService.Object,
//            _mockAddFlexibleWorkerInfoUsecase.Object
//        );

//        var claims = new List<Claim> { new Claim("sub", employerId.ToString()) };
//        controller.ControllerContext = new ControllerContext
//        {
//            HttpContext = new DefaultHttpContext
//            {
//                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
//            }
//        };

//        return controller;
//    }

//    [Fact]
//    public async Task GetWorkerInfosByEmail_ValidEmail_ReturnsWorkerInfoDto()
//    {
//        // Arrange
//        var fakeEmployerId = Guid.NewGuid();
//        var employerController = CreateController(fakeEmployerId);

//        var email = "test@email";
//        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana", Id = Guid.NewGuid() };
//        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456", Id = Guid.NewGuid() };
//        var workerInfos = new List<WorkerInfo> { worker_info1, worker_info2 };
//        _mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email, fakeEmployerId)).ReturnsAsync(workerInfos);

//        // Act
//        var okResult = await employerController.GetWorkerInfosByEmail(email);

//        // Assert
//        var result = Assert.IsType<OkObjectResult>(okResult.Result);
//        var resultList = Assert.IsType<List<WorkerInfoDto>>(result.Value);
//        Assert.Equal(2, resultList.Count);
//        Assert.Equal(worker_info1.Id, resultList[0].Id);
//        Assert.Equal("address", resultList[0].Desc);
//        Assert.Equal(worker_info2.Id, resultList[1].Id);
//        Assert.Equal("phone", resultList[1].Desc);
//    }

//    [Fact]
//    public async Task GetWorkerInfosByEmail_InValidEmail_ReturnsEmptyList()
//    {
//        // Arrange
//        var fakeEmployerId = Guid.NewGuid();
//        var employerController = CreateController(fakeEmployerId);

//        var email = "test@email";
//        var workerInfos = new List<WorkerInfo>();
//        _mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email, fakeEmployerId)).ReturnsAsync(workerInfos);

//        // Act
//        var Result = await employerController.GetWorkerInfosByEmail(email);

//        // Assert
//        var result = Assert.IsType<OkObjectResult>(Result.Result);
//        var resultList = Assert.IsType<List<WorkerInfoDto>>(result.Value);
//        Assert.Empty(resultList);
//    }

//    [Fact]
//    public async Task CreateDataAccessRequest_ValidInput_ReturnsOk()
//    {
//        // Arrange
//        var fakeEmployerId = Guid.NewGuid();
//        var employerController = CreateController(fakeEmployerId);

//        var email = "test@email";
//        var fakeRequest = new Request { EmployerId = fakeEmployerId, WorkerId = Guid.NewGuid(), Reason = "check basic info" };
//        var worker_info1 = new WorkerInfo { Desc = "address", Value = "havana rise", WorkerId = fakeRequest.WorkerId };
//        var worker_info2 = new WorkerInfo { Desc = "phone", Value = "123456", WorkerId = fakeRequest.WorkerId };
//        var workerInfos = new List<WorkerInfo> { worker_info1, worker_info2 };
//        var infoDesc = new List<string> { worker_info1.Id.ToString(), worker_info2.Id.ToString() };

//        _mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email, fakeEmployerId)).ReturnsAsync(workerInfos);
//        _mockCreateDataUsecase.Setup(r => r.CreateDataAccessRequest(workerInfos, fakeRequest.EmployerId, fakeRequest.WorkerId, fakeRequest.Reason)).Returns(Task.CompletedTask);

//        var fakeRequestDTO = new CreateRequestUsecaseDTO { Email = email, InfoDesc = infoDesc, Reason = fakeRequest.Reason };

//        // Act
//        var okResult = await employerController.CreateRequest(fakeRequestDTO);

//        // Assert
//        Assert.IsType<OkResult>(okResult);
//    }

//    [Fact]
//    public async Task CreateDataAccessRequest_EmptyWorkerInfos_Returns404()
//    {
//        // Arrange
//        var employerId = Guid.NewGuid();
//        var employerController = CreateController(employerId);

//        var email = "test@email";
//        var infoDesc = new List<string>();
//        var workerInfos = new List<WorkerInfo>();
//        _mockFindInfosUsecase.Setup(r => r.FindWorkerInfosByEmail(email, default)).ReturnsAsync(workerInfos);

//        var fakeRequestDTO = new CreateRequestUsecaseDTO { Email = email, InfoDesc = infoDesc, Reason = "check the basic info" };

//        // Act
//        var Result = await employerController.CreateRequest(fakeRequestDTO);

//        // Assert
//        Assert.IsType<NotFoundResult>(Result);
//    }
//}
