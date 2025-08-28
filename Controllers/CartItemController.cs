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

                return Ok(new { success = true, message = "Item added to cart", orderId = cartItem.Id });
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
                                      ci.Product.DiscountPercentage,
                                      ci.Product.MinOrderQuantity,
                                      ci.Product.StockQuantity
                                  })
                                  .Select(g => new CartItemResponseDto
                                  {
                                      Id = g.Key.ProductId,
                                      Name = g.Key.Name,
                                      Price = g.Key.Price,
                                      Image = g.Key.Image,
                                      MinOrderQuantity = g.Key.MinOrderQuantity,
                                      Stock = g.Key.StockQuantity,
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

        [HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateQuantity(CreateCartItemDto dto)
        {
            try
            {
                if (dto.Quantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Quantity must be greater than 0" });
                }

                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null && string.IsNullOrEmpty(dto.GuestId))
                    return BadRequest(new { success = false, message = "Either a logged-in user or GuestId must be provided." });

                var cartItem = await dbContext.CartItems
                                        .Where(c =>
                                            c.ProductId == dto.ProductId &&
                                            c.IsActive &&
                                            (
                                                (!string.IsNullOrEmpty(dto.GuestId) && c.GuestId == dto.GuestId) ||
                                                (userId != null && c.UserId == userId.Value)
                                            )
                                        )
                                        .FirstOrDefaultAsync();

                if (cartItem == null)
                {
                    return NotFound(new { success = false, message = "Cart item not found" });
                }

                cartItem.Quantity = dto.Quantity;


                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Product Quantity updated" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveItem(int id, [FromQuery] string? guestId)
        {
            try
            {
                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null && string.IsNullOrEmpty(guestId))
                {
                    return BadRequest(new { success = false, message = "Either a logged-in user or GuestId must be provided." });
                }

                var cartItem = await dbContext.CartItems
                    .Where(c => c.ProductId == id
                             && c.IsActive
                             && ((guestId != null && c.GuestId == guestId) || (userId != null && c.UserId == userId)))
                    .FirstOrDefaultAsync();

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

        [HttpDelete("clear")]
        [AllowAnonymous]
        public async Task<IActionResult> ClearCart([FromQuery] string? guestId)
        {
            try
            {
                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null && string.IsNullOrEmpty(guestId))
                {
                    return BadRequest(new { success = false, message = "Either a logged-in user or GuestId must be provided." });
                }

                var cartItems = await dbContext.CartItems
                    .Where(c => ((guestId != null && c.GuestId == guestId) || (userId != null && c.UserId == userId)) && c.IsActive)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return NotFound(new { success = false, message = "Cart is already empty" });
                }

                foreach (var item in cartItems)
                {
                    item.IsActive = false;
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

    }
}
