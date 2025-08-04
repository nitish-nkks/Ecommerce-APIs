using System.Security.Claims;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
using Ecommerce_APIs.Models.DTOs.OrderDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public OrderController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null)
                    return Unauthorized(new { success = false, message = "Invalid token or user not found" });

                Customer? customer = null;
                InternalUser? staff = null;
                if (userType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
                {
                    customer = await dbContext.Customers.FindAsync(userId.Value);
                    if (customer == null)
                        return BadRequest(new { success = false, message = $"Customer with ID {userId} does not exist." });
                }
                else
                {
                    staff = await dbContext.InternalUsers.FindAsync(userId.Value);
                    if (staff == null)
                        return BadRequest(new { success = false, message = $"Internal user with ID {userId} does not exist." });
                }
                var cartItems = await dbContext.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId.Value && c.UserType == userType && c.IsActive)
                    .ToListAsync();

                if (!cartItems.Any())
                    return BadRequest(new { success = false, message = "Cart is empty. Cannot create order." });

                foreach (var item in cartItems)
                {
                    var product = item.Product;
                    if (item.Quantity < product.MinOrderQuantity)
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Minimum order quantity for '{product.Name}' is {product.MinOrderQuantity}."
                        });
                    if (item.Quantity > product.MaxOrderQuantity)
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Maximum order quantity for '{product.Name}' is {product.MaxOrderQuantity}."
                        });
                    if (item.Quantity > product.StockQuantity)
                        return BadRequest(new
                        {
                            success = false,
                            message = $"Only {product.StockQuantity} units available for '{product.Name}'."
                        });
                }

                decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);

                var order = new Order
                {
                    CustomerId = customer?.Id,
                    InternalUserId = staff?.Id,
                    ShippingAddress = dto.ShippingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    CreatedBy = userId.Value,
                    CreatedByUserType = userType,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    OrderItems = cartItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price,
                        TotalPrice = item.Product.Price * item.Quantity
                    }).ToList()
                };

                foreach (var item in cartItems)
                    item.Product.StockQuantity -= item.Quantity;

                dbContext.Orders.Add(order);
                dbContext.CartItems.RemoveRange(cartItems);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, orderId = order.Id, message = "Order created successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("user/{customerId}")]
        public IActionResult GetOrdersByUser(int customerId)
        {
            try
            {
                var orders = dbContext.Orders
                    .Where(o => o.CustomerId == customerId && o.IsActive)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.ProductImages)
                    .ToList();

                if (!orders.Any())
                    return NotFound(new { success = false, message = "No orders found for this user." });

                var response = orders.Select(order => new OrderResponseDto
                {
                    Id = order.Id,
                    ShippingAddress = order.ShippingAddress,
                    PaymentMethod = order.PaymentMethod,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status.ToString(),
                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductImageUrl = oi.Product.ProductImages.FirstOrDefault()?.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                }).ToList();

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to retrieve orders", error = ex.Message });
            }
        }

        [HttpGet("{orderId}")]
        public IActionResult GetOrderById(int orderId)
        {
            try
            {
                var order = dbContext.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(p => p.ProductImages)
                    .FirstOrDefault(o => o.Id == orderId && o.IsActive);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                var response = new OrderResponseDto
                {
                    Id = order.Id,
                    ShippingAddress = order.ShippingAddress,
                    PaymentMethod = order.PaymentMethod,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status.ToString(),
                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductImageUrl = oi.Product.ProductImages.FirstOrDefault()?.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                };

                return Ok(new { success = true, data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving order", error = ex.Message });
            }
        }

        [HttpDelete("{orderId}")]
        public IActionResult CancelOrder(int orderId)
        {
            try
            {
                var order = dbContext.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.Id == orderId && o.IsActive);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (order.Status != OrderStatus.Pending)
                    return BadRequest(new { success = false, message = "Only pending orders can be cancelled." });

                order.IsActive = false;
                order.UpdatedBy = TokenHelper.GetUserIdFromClaims(User);
                order.UpdatedAt = DateTime.Now;

                dbContext.SaveChanges();

                return Ok(new { success = true, message = "Order cancelled successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error cancelling order", error = ex.Message });
            }
        }

        [HttpPut("{orderId}/status")]
        public IActionResult UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId && o.IsActive);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!Enum.TryParse<OrderStatus>(statusDto.Status, true, out var newStatus))
                    return BadRequest(new { success = false, message = "Invalid status value." });

                order.Status = newStatus;
                order.UpdatedBy = TokenHelper.GetUserIdFromClaims(User);
                order.UpdatedAt = DateTime.Now;

                dbContext.SaveChanges();

                return Ok(new { success = true, message = "Order status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating order status", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await dbContext.Orders
                    .Where(o => o.IsActive)
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .ToListAsync();

                var result = orders.Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.Status,
                    o.TotalAmount,
                    User = new
                    {
                        o.Customer.Id,
                        o.Customer.FirstName,
                        o.Customer.LastName,
                        o.Customer.Email
                    },
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.ProductId,
                        ProductName = oi.Product.Name,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.TotalPrice
                    })
                });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to retrieve orders", error = ex.Message });
            }
        }
    }
}

