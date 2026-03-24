using Microsoft.AspNetCore.Mvc;
using minibank.Controllers;
using minibank.DTOs;
using minibank.Services.Interfaces;
using minibank.Wrappers;
using Moq;
using Xunit;

namespace minibank.Tests.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task GetAccountDetails_ShouldReturnOk_WhenAccountExists()
        {
            var mockService = new Mock<IAccountService>();
            var testAccountId = 1;
            var fakeAccount = new AccountDto{Id = testAccountId,AccountNumber= "AL123"};

            mockService.Setup(s=>s.GetAccountDetailsAsync(testAccountId))
            .ReturnsAsync(ApiResponse<AccountDto>.SuccessResponse(fakeAccount));

            var contoller  = new AccountController(mockService.Object);

            var result = await contoller.GetAccountDetails(testAccountId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = Assert.IsType<ApiResponse<AccountDto>>(okResult.Value);
            Assert.NotNull(returnedValue.Data);
            Assert.True(returnedValue.Success);
            Assert.Equal("AL123", returnedValue.Data!.AccountNumber);
        }

        [Fact]
        public async Task TransferFunds_ShouldReturnOk_WhenTransferSucceeds()
        {
            var mockService = new Mock<IAccountService>();
            var dto = new TransferDto { FromAccountId = 1, ToAccountId = 2, Amount = 25m };

            mockService.Setup(s => s.TransferAsync(dto))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true, "Transfer completed successfully."));

            var controller = new AccountController(mockService.Object);

            var result = await controller.TransferFunds(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedValue = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(returnedValue.Success);
            Assert.True(returnedValue.Data);
        }

        [Fact]
        public async Task TransferFunds_ShouldReturnBadRequest_WhenTransferFails()
        {
            var mockService = new Mock<IAccountService>();
            var dto = new TransferDto { FromAccountId = 1, ToAccountId = 2, Amount = 25m };

            mockService.Setup(s => s.TransferAsync(dto))
                .ReturnsAsync(ApiResponse<bool>.FailureResponse("A system error occured during the transfer. No funds were moved."));

            var controller = new AccountController(mockService.Object);

            var result = await controller.TransferFunds(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var returnedValue = Assert.IsType<ApiResponse<bool>>(badRequest.Value);
            Assert.False(returnedValue.Success);
            Assert.Equal("A system error occured during the transfer. No funds were moved.", returnedValue.Message);
        }
    }
}
