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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _customerService.GetAllCustomersAsync());
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search ([FromQuery] string name)
        {
            return Ok(await _customerService.SearchCustomersAsync(name));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            if(!result.Success)return NotFound(result);
            return Ok(result);
        }
    }
}