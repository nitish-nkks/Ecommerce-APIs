using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
using Ecommerce_APIs.Models.Entites;
using Ecommerce_APIs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Email && c.IsActive);

            if (customer != null)
            {
                if (!PasswordHasherHelper.VerifyPassword(customer.PasswordHash, request.Password))
                    return Unauthorized(new { message = "Invalid email or password" });

                if (!string.IsNullOrEmpty(request.GuestId))
                {
                    var guestCartItems = await _context.CartItems
                        .Where(c => c.GuestId == request.GuestId && c.CustomerId == null)
                        .ToListAsync();

                    foreach (var item in guestCartItems)
                    {
                        item.CustomerId = customer.Id;
                        item.GuestId = null;
                    }

                    await _context.SaveChangesAsync();
                }

                customer.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(
                    customer.Id.ToString(),
                    customer.Email,
                    "Customer",
                    "Customer"
                );

                return Ok(new
                {
                    token,
                    role = "Customer",
                    user = new
                    {
                        customer.Id,
                        customer.FirstName,
                        customer.LastName,
                        customer.Email
                    }
                });
            }

            var internalUser = await _context.InternalUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (internalUser != null)
            {
                if (!PasswordHasherHelper.VerifyPassword(internalUser.PasswordHash, request.Password))
                    return Unauthorized(new { message = "Invalid email or password" });

                internalUser.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var token = _jwtService.GenerateToken(
                    internalUser.Id.ToString(),
                    internalUser.Email,
                    internalUser.Role.ToString(),
                    "InternalUser"
                );

                return Ok(new
                {
                    token,
                    role = internalUser.Role.ToString(),
                    user = new
                    {
                        internalUser.Id,
                        internalUser.UserName,
                        internalUser.Email,
                        internalUser.Role
                    }
                });
            }

            return Unauthorized(new { message = "Invalid email or password" });
        }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? GuestId { get; set; }
    }
}
