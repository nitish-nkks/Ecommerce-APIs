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
        [Authorize]
        public async Task<IActionResult> PostOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var (userId, userType) = TokenHelper.GetUserInfoFromClaims(User);

                if (userId == null || !string.Equals(userType, "Customer", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(403, new { success = false, message = "Only customers are authorized to place orders." });
                }

                var customer = await dbContext.Customers.FindAsync(userId.Value);
                if (customer == null)
                {
                    return BadRequest(new { success = false, message = $"Customer with ID {userId} does not exist." });
                }

                var cartItems = await dbContext.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId.Value && c.UserType == "Customer")
                    .ToListAsync();

                if (!cartItems.Any())
                    return BadRequest(new { success = false, message = "Cart is empty. Cannot create order." });

                foreach (var item in cartItems)
                {
                    var product = item.Product;
                    if (item.Quantity < product.MinOrderQuantity)
                        return BadRequest(new { success = false, message = $"Minimum order quantity for '{product.Name}' is {product.MinOrderQuantity}." });
                    if (item.Quantity > product.MaxOrderQuantity)
                        return BadRequest(new { success = false, message = $"Maximum order quantity for '{product.Name}' is {product.MaxOrderQuantity}." });
                    if (item.Quantity > product.StockQuantity)
                        return BadRequest(new { success = false, message = $"Only {product.StockQuantity} units available for '{product.Name}'." });
                }

                if (dto.CustomerAddressId <= 0)
                {
                    return BadRequest(new { success = false, message = "Shipping address ID is required." });
                }

                var shippingAddress = await dbContext.CustomerAddresses
                    .FirstOrDefaultAsync(a =>
                        a.Id == dto.CustomerAddressId &&
                        a.CustomerId == userId.Value && 
                        a.IsActive);

                if (shippingAddress == null)
                    return BadRequest(new { success = false, message = "Invalid or unauthorized shipping address provided." });

                decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
                string formattedAddress = $"{shippingAddress.AddressLine}, {shippingAddress.City}, {shippingAddress.State} - {shippingAddress.ZipCode}";

                var order = new Order
                {
                    CustomerId = customer.Id,
                    CustomerAddressId = shippingAddress.Id,
                    ShippingAddress = formattedAddress,
                    PaymentMethod = dto.PaymentMethod,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Order_Placed,
                    CreatedBy = userId.Value,
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

                dbContext.Orders.Add(order);
                await dbContext.SaveChangesAsync();
                dbContext.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = OrderStatus.Order_Placed,
                    Remarks = "Order placed by customer.",
                    ChangedByUserId = userId,
                    ChangedByUserType = userType ?? "Customer",
                    ChangedAt = DateTime.Now
                });

                dbContext.CartItems.RemoveRange(cartItems);

                foreach (var item in cartItems)
                {
                    item.Product.StockQuantity -= item.Quantity;
                }

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
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var userId = TokenHelper.GetUserIdFromClaims(User);
                var userType = TokenHelper.GetUserTypeFromClaims(User);

                var order = dbContext.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.Id == orderId && o.IsActive);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                {
                    return BadRequest(new { success = false, message = $"Cannot cancel an order that is {order.Status}." });
                }

                dbContext.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = OrderStatus.Cancelled,
                    Remarks = "Cancelled by user",
                    ChangedByUserId = userId,
                    ChangedByUserType = userType ?? "Unknown"
                });
                order.Status = OrderStatus.Cancelled;
                order.IsActive = false;
                order.UpdatedBy = TokenHelper.GetUserIdFromClaims(User);
                order.UpdatedAt = DateTime.Now;

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Order cancelled successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error cancelling order", error = ex.Message });
            }
        }

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId && o.IsActive);
                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!Enum.TryParse<OrderStatus>(statusDto.Status, true, out var newStatus))
                    return BadRequest(new { success = false, message = "Invalid status value." });

                var userId = TokenHelper.GetUserIdFromClaims(User);
                var userType = TokenHelper.GetUserTypeFromClaims(User);

                if (order.Status != newStatus)
                {
                    dbContext.OrderStatusHistories.Add(new OrderStatusHistory
                    {
                        OrderId = orderId,
                        Status = newStatus,
                        Remarks = statusDto.Remarks,
                        ChangedByUserId = userId,
                        ChangedByUserType = userType ?? "Unknown"
                    });
                }

                order.Status = newStatus;
                order.UpdatedBy = userId;
                order.UpdatedAt = DateTime.Now;

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Order status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating order status", error = ex.Message });
            }
        }

        [HttpGet("{orderId}/status-history")]
        public IActionResult GetOrderStatusHistory(int orderId)
        {
            try {
                var history = dbContext.OrderStatusHistories
                .Where(h => h.OrderId == orderId)
                .OrderBy(h => h.ChangedAt)
                .Select(h => new
                {
                    h.Status,
                    h.Remarks,
                    h.ChangedByUserId,
                    h.ChangedByUserType,
                    h.ChangedAt
                }).ToList();

                return Ok(new { success = true, data = history });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing the get order request.",
                    error = ex.Message
                });
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

        [HttpPost("{orderId}/return")]
        public async Task<IActionResult> RequestReturn(int orderId, [FromBody] ReturnRequestDto dto)
        {
            try
            {
                var userId = TokenHelper.GetUserIdFromClaims(User);
                var userType = TokenHelper.GetUserTypeFromClaims(User);

                var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.IsActive);
                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (order.Status != OrderStatus.Delivered)
                    return BadRequest(new { success = false, message = "Only delivered orders can be returned." });

                order.Status = OrderStatus.Return_Requested;
                order.UpdatedAt = DateTime.Now;
                order.UpdatedBy = userId;

                dbContext.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = OrderStatus.Return_Requested,
                    Remarks = dto.Reason,
                    ChangedByUserId = userId,
                    ChangedByUserType = userType ?? "Unknown",
                    ChangedAt = DateTime.Now
                });

                var returnRequest = new OrderReturnRequest
                {
                    OrderId = order.Id,
                    Reason = dto.Reason,
                    RequestedBy = userId,
                    RequestedByUserType = userType ?? "Unknown",
                    RequestedAt = DateTime.Now
                };

                dbContext.OrderReturnRequests.Add(returnRequest);
                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "Return request submitted successfully." });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while processing the return request.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("returns/{id}/status")]
        public async Task<IActionResult> UpdateReturnStatus(int id, [FromBody] ReturnStatusUpdateDto dto)
        {
            try
            {
                var userId = TokenHelper.GetUserIdFromClaims(User);
                var userType = TokenHelper.GetUserTypeFromClaims(User);

                var request = await dbContext.OrderReturnRequests.FindAsync(id);
                if (request == null)
                    return NotFound(new { success = false, message = "Return request not found." });

                if (!Enum.TryParse<ReturnStatus>(dto.Status, true, out var newStatus))
                    return BadRequest(new { success = false, message = "Invalid return status value." });

                request.Status = newStatus;
                request.AdminRemarks = dto.AdminRemarks;
                request.ActionedAt = DateTime.Now;
                request.ActionedBy = userId;
                request.ActionedByUserType = userType ?? "Unknown";

                if (newStatus == ReturnStatus.Approved)
                {
                    var order = await dbContext.Orders.FindAsync(request.OrderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Returned;
                        order.UpdatedAt = DateTime.Now;
                        order.UpdatedBy = userId;
                    }
                }

                await dbContext.SaveChangesAsync();

                return Ok(new { success = true, message = $"Return request {newStatus} successfully." });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating return request status.",
                    error = ex.Message
                });
            }
        }



    }
}

