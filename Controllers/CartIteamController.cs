using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.CartIteamsDtos;
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
    public class CartIteamController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public CartIteamController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(CreateCartItemDto dto)
        {
            try
            {
                var product = await dbContext.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                var cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    UserId = dto.UserId,
                    Quantity = dto.Quantity
                };

                dbContext.CartItems.Add(cartItem);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Item added to cart" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCartItems(int userId)
        {
            try
            {
                var cartItems = await dbContext.CartItems
                    .Where(ci => ci.UserId == userId)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                    .ToListAsync();

                var response = cartItems.Select(ci => new CartItemResponseDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImageUrl = ci.Product.ProductImages.FirstOrDefault()?.ImageUrl,
                    UnitPrice = ci.Product.Price,
                    Quantity = ci.Quantity
                }).ToList();

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var cartItem = await dbContext.CartItems.FindAsync(id);
                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Cart item not found" });
                }

                cartItem.Quantity = quantity;
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Quantity updated" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            try
            {
                var cartItem = await dbContext.CartItems.FindAsync(id);
                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Cart item does not exist" });
                }

                dbContext.CartItems.Remove(cartItem);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Item removed from cart" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }
    }
}
