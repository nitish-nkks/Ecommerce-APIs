using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
using Ecommerce_APIs.Models.DTOs.CustomerAddressDtos;
using Ecommerce_APIs.Models.Entites;
using Ecommerce_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerAddressesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly PincodeService pincodeService;

        public CustomerAddressesController(ApplicationDbContext dbContext,PincodeService pincodeService)
        {
            this.dbContext = dbContext;
            this.pincodeService = pincodeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerAddresses()
        {
            var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

            if (userId == null || !userType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user type." });
            }

            var addresses = await dbContext.CustomerAddresses
                .Where(a => a.CustomerId == userId.Value && a.IsActive)
                .ToListAsync();

            return Ok(new { success = true, data = addresses });
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomerAddress([FromBody] AddressDto dto)
        {
            var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

            if (userId == null || !userType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { success = false, message = "Invalid token or user type." });
            }

            var pincodeCheckResult = await pincodeService.CheckPincodeAsync(dto.ZipCode);
            if (!pincodeCheckResult.Deliverable)
            {
                return BadRequest(new { success = false, message = "Sorry, we don't deliver to this area. Please use a different address." });
            }

            var customer = await dbContext.Customers.FindAsync(userId.Value);
            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found." });
            }

            var newAddress = new CustomerAddress
            {
                CustomerId = userId.Value,
                AddressLine = dto.AddressLine,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                IsActive = true
            };

            dbContext.CustomerAddresses.Add(newAddress);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomerAddresses), new { id = newAddress.Id }, new { success = true, message = "Address added successfully.", data = newAddress });
        }
    }
}