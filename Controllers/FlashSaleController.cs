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
    [Authorize]
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
                return StatusCode(500, new {
                    message = "An error occurred while creating the flash sale.",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public IActionResult GetActiveFlashSales()
        {
            try{

                var today = DateTime.Today;
                var now = DateTime.Now.TimeOfDay; 
                var currentDay = (int)DateTime.Now.DayOfWeek;

                var sales = _context.FlashSales
                    .Include(fs => fs.Product)
                        .ThenInclude(p => p.Category)
                    .Where(fs => fs.IsActive &&
                        (
                            (fs.StartDate <= today && fs.EndDate >= today)
                            ||
                            (fs.SaleDay != null && fs.StartTime.HasValue && fs.EndTime.HasValue &&
                             (int)fs.SaleDay.Value == currentDay &&
                             fs.StartTime.Value <= now &&
                             fs.EndTime.Value >= now)
                       )
                    )
                    .Select(fs => new
                    {
                        Id = fs.Id,
                        fs.ProductId,
                        Name = fs.Product.Name,
                        ParentCategory = fs.Product.Category.ParentCategoryId == null
                            ? fs.Product.Category.Name
                            : (fs.Product.Category.ParentCategory.ParentCategoryId == null)
                                ? fs.Product.Category.ParentCategory.Name
                                : fs.Product.Category.ParentCategory.ParentCategory.Name,
                        Image = fs.Product.Image,
                        Price = fs.Product.Price,
                        OriginalPrice = fs.Product.Price,
                        SalePrice = fs.Product.Price - (fs.Product.Price * fs.DiscountPercent / 100),
                        Discount = fs.DiscountPercent,
                        Stock = fs.Product.StockQuantity,
                        fs.SaleDay,
                        fs.StartDate,
                        fs.EndDate,
                        fs.StartTime,
                        fs.EndTime                      
                    })
                    .ToList();


                return Ok(new
                {
                    Succeeded = true,
                    Message = "Flash sales retrieved successfully.",
                    Data = sales
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { message = "An error occurred while retrieving active flash sales." });
            }
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleFlashSaleActiveStatus(int id)
        {
            var flashSale = await _context.FlashSales.FindAsync(id);
            if (flashSale == null) return NotFound(new { message = "Flash sale not found." });

            flashSale.IsActive = !flashSale.IsActive;
            flashSale.UpdatedAt = DateTime.Now;
            flashSale.UpdatedBy = GetCurrentUserId();

            await _context.SaveChangesAsync();
            return Ok(new { message = "Flash sale status updated.", isActive = flashSale.IsActive });
        }



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
