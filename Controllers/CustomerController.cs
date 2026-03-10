using minibank.DTOs;
using minibank.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace minibank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateCustomerDto dto)
        {
            var result = await _customerService.RegisterCustomerAsync(dto);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }
    }
}