using System.Security.Claims;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
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
    public class CartItemController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public CartItemController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddToCart(CreateCartItemDto dto)
        {
            try
            {
                var product = await dbContext.Products
                    //.Include(p => p.Image)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

                if (product == null)
                {
                    return NotFound(new { success = false, message = "Product not found" });
                }

                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null && string.IsNullOrEmpty(dto.GuestId))
                    return BadRequest(new { success = false, message = "Either a logged-in user or GuestId must be provided." });

                var cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UserId = userId,
                    GuestId = userId == null ? dto.GuestId : null,
                    UserType = userId == null ? "Guest" : userType ?? "Unknown",
                    AddedAt = DateTime.Now
                };

                dbContext.CartItems.Add(cartItem);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Item added to cart" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCartItems(int userId)
        {
            try
            {
                var cartItems = await dbContext.CartItems
                    .Where(ci => ci.UserId == userId && ci.IsActive)
                    .Include(ci => ci.Product)
                        //.ThenInclude(p => p.Image)
                    .ToListAsync();

                var response = cartItems
                                  .GroupBy(ci => new
                                  {
                                      ci.ProductId,
                                      ci.Product.Name,
                                      ci.Product.Price,
                                      ci.Product.Image,
                                      ci.Product.DiscountPercentage
                                  })
                                  .Select(g => new CartItemResponseDto
                                  {
                                      Id = g.Key.ProductId,
                                      Name = g.Key.Name,
                                      Price = g.Key.Price,
                                      Image = g.Key.Image,
                                      Discount = g.Key.DiscountPercentage,
                                      Quantity = g.Sum(ci => ci.Quantity)
                                  })
                                  .ToList();

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
                var cartItem = await dbContext.CartItems
                    .Where(c => c.Id == id && c.IsActive) 
                    .FirstOrDefaultAsync();

                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Cart item not found" });
                }

                cartItem.Quantity = quantity;

                if (quantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Quantity must be greater than 0" });
                }

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

                cartItem.IsActive = false;
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
