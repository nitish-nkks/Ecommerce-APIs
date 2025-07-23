using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.OrderDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                var user = await dbContext.userss.FindAsync(dto.UserId);
                if (user == null)
                    return BadRequest(new { success = false, message = $"User with ID {dto.UserId} does not exist." });

                var cartItems = await dbContext.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.UserId == dto.UserId && c.IsActive)
                    .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                    return BadRequest(new { success = false, message = "Cart is empty. Cannot create order." });

                decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);

                var order = new Order
                {
                    UserId = dto.UserId,
                    ShippingAddress = dto.ShippingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    CreatedBy = dto.UserId,
                    CreatedAt = DateTime.UtcNow,
                    OrderItems = cartItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price,
                        TotalPrice = item.Product.Price * item.Quantity
                    }).ToList()
                };

                dbContext.Orders.Add(order);
                dbContext.CartItems.RemoveRange(cartItems);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, orderId = order.Id, message = "Order created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetOrdersByUser(int userId)
        {
            try
            {
                var orders = dbContext.Orders
                    .Where(o => o.UserId == userId)
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
                    .FirstOrDefault(o => o.Id == orderId);

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
        public IActionResult DeleteOrder(int orderId)
        {
            try
            {
                var order = dbContext.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (order.Status != OrderStatus.Pending)
                    return BadRequest(new { success = false, message = "Only pending orders can be cancelled." });

                dbContext.OrderItems.RemoveRange(order.OrderItems);
                dbContext.Orders.Remove(order);
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
                var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!Enum.TryParse<OrderStatus>(statusDto.Status, true, out var newStatus))
                    return BadRequest(new { success = false, message = "Invalid status." });

                order.Status = newStatus;
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
                    .Include(o => o.User)
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
                        o.User.Id,
                        o.User.FirstName,
                        o.User.LastName,
                        o.User.Email
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
                return StatusCode(500, new { success = false, message = "Failed to retrieve all orders", error = ex.Message });
            }
        }
    }
}
