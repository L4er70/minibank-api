using minibank.DTOs;
using minibank.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using minibank.Wrappers;

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

        [HttpPatch("{accountId}/reopen")]
        public async Task<IActionResult> ReopenAccount(int accountId)
        {
            var result = await _accountService.ReopenAccountAsync(accountId);
            return result.Success ? Ok(result): BadRequest(result);
        }

        [HttpPatch("{accountId}/close")]
        public async Task<IActionResult> CloseAccount(int accountId)
        {
            var result = await _accountService.CloseAccountAsync(accountId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
            
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

        [HttpGet("resolve/{accountNumber}")]
        public async Task<IActionResult> ResolveAccount(string accountNumber)
        {
            var result = await _accountService.ResolveAccountAsync(accountNumber);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> CreateTransaction([FromBody] PostTransactionDto dto)
        {
            var result = await _accountService.CreateTransactionAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var result = await _accountService.CreateAccountAsync(dto);
            if(!result.Success)return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferFunds([FromBody] TransferDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.FailureResponse("Invalid transfer data."));

            }
            var result = await _accountService.TransferAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("transfer-external")]
        public async Task<IActionResult> TransferToExternal([FromBody] CrossCustomerTransferDto dto)
        {
            if(!ModelState.IsValid)return BadRequest(ApiResponse<bool>.FailureResponse("Invalid data."));
            var result = await _accountService.TransferToCustomerAsync(dto);

            if(!result.Success) return BadRequest(result);
            return Ok(result);
        }


        

        
    }

   
}
