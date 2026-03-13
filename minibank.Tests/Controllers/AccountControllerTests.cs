using Microsoft.AspNetCore;
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
            Assert.True(returnedValue.Success);
            Assert.Equal("AL123", returnedValue.Data.AccountNumber);
        }
    }
}