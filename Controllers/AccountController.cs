using minibank.DTOs;
using minibank.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace minibank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

         public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetAccountByCustomerId(int customerId)
        {
            var result = await _accountService.GetAccountByCustomerIdAsync(customerId);
            if(!result.Success)return NotFound(result);
            return Ok(result);
            
        }

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAccountDetails(int accountId)
        {
            var result  = await _accountService.GetAccountDetailsAsync(accountId);
            if(!result.Success)return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{accountId}/transactions")]
        public async Task<IActionResult> GetTransaction(int accountId)
        {
            var result = await _accountService.GetAccountTransactionAsync(accountId);
            if(!result.Success)return NotFound(result);
            return Ok(result);
        }
    }

   
}