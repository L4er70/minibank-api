using minibank.Data;
using minibank.DTOs;
using minibank.Models;
using minibank.Services;
using Microsoft.EntityFrameworkCore;
using minibank.Wrappers;
using minibank.Services.Interfaces;

namespace minibank.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly BankingDbContext _context;
        public CustomerService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Customer>> RegisterCustomerAsync(CreateCustomerDto dto)
        {
            
            var exists = await _context.Customers.AnyAsync(c=>c.PersonalId ==dto.PersonalId);
            if(exists)return ApiResponse<Customer>.FailureResponse("A customer with this Personal ID is already registered");

            var customer = new Customer
            {
                PersonalId = dto.PersonalId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return ApiResponse<Customer>.SuccessResponse(customer);

            //throw new NotImplementedException();
        }
    }
}