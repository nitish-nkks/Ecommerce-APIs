using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
using Ecommerce_APIs.Models.DTOs.CustomerAddressDtos;
using Ecommerce_APIs.Models.Entites;
using Ecommerce_APIs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerAddressesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly PincodeService pincodeService;

        public CustomerAddressesController(ApplicationDbContext dbContext, PincodeService pincodeService)
        {
            this.dbContext = dbContext;
            this.pincodeService = pincodeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerAddresses()
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomerAddress([FromBody] AddressDto dto)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("{addressId}")]
        public async Task<IActionResult> UpdateCustomerAddress(int addressId, [FromBody] AddressDto dto)
        {
            try
            {
                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null || !userType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { success = false, message = "Invalid token or user type." });
                }

                var addressToUpdate = await dbContext.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == userId.Value && a.IsActive);

                if (addressToUpdate == null)
                {
                    return NotFound(new { success = false, message = "Address not found or unauthorized to update." });
                }

                var pincodeCheckResult = await pincodeService.CheckPincodeAsync(dto.ZipCode);
                if (!pincodeCheckResult.Deliverable)
                {
                    return BadRequest(new { success = false, message = "Sorry, we don't deliver to the new area. Please use a different address." });
                }

                addressToUpdate.AddressLine = dto.AddressLine;
                addressToUpdate.City = dto.City;
                addressToUpdate.State = dto.State;
                addressToUpdate.ZipCode = dto.ZipCode;

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Address updated successfully.", data = addressToUpdate });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("{addressId}")]
        public async Task<IActionResult> DeleteCustomerAddress(int addressId)
        {
            try
            {
                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null || !userType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { success = false, message = "Invalid token or user type." });
                }

                var addressToDelete = await dbContext.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == userId.Value && a.IsActive);

                if (addressToDelete == null)
                {
                    return NotFound(new { success = false, message = "Address not found." });
                }

                addressToDelete.IsActive = false;
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Address deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }
    }
}