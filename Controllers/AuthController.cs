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
            var user = await _context.users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            if (!PasswordHasherHelper.VerifyPassword(user.PasswordHash, request.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            if (!string.IsNullOrEmpty(request.GuestId))
            {
                var guestCartItems = await _context.CartItems
                    .Where(c => c.GuestId == request.GuestId && c.UserId == null)
                    .ToListAsync();

                foreach (var item in guestCartItems)
                {
                    item.UserId = user.Id;
                    item.GuestId = null;
                }

                await _context.SaveChangesAsync();
            }

            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role.ToString());

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Role
                }
            });
        }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? GuestId { get; set; } = null;
    }
}
