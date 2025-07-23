using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Mappings;
using Ecommerce_APIs.Models.DTOs.FlashSaleDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FlashSaleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public FlashSaleController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFlashSale([FromBody] FlashSaleDto dto)
        {
            try
            {
                var flashSale = _mapper.Map<FlashSale>(dto);
                flashSale.CreatedBy = GetCurrentUserId();

                _context.FlashSales.Add(flashSale);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Flash sale created successfully." });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { message = "An error occurred while creating the flash sale." });
            }
        }

        [HttpGet("active")]
        public IActionResult GetActiveFlashSales()
        {
            try
            {
                var now = DateTime.UtcNow;

                var sales = _context.FlashSales
                    .Include(fs => fs.Product)
                    .Where(fs => fs.StartDate <= now && fs.EndDate >= now)
                    .ToList();

                return Ok(sales);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { message = "An error occurred while retrieving active flash sales." });
            }
        }

        // Helper method to get the current user's ID from the claims principal
        private int? GetCurrentUserId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId" || c.Type == "id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }
    }
}
