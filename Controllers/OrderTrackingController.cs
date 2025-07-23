using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.OrderTrackingDtos;
using Ecommerce_APIs.Models.Entites.Ecommerce_APIs.Models.Entities;
using Ecommerce_APIs.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderTrackingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderTrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/OrderTracking
        [HttpPost]
        public async Task<IActionResult> CreateOrderTracking([FromBody] CreateOrderTrackingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid input", errors = ModelState });

                var orderTracking = new OrderTracking
                {
                    OrderId = dto.OrderId,
                    UserId = dto.UserId,
                    TrackingId = dto.TrackingId,
                    Remarks = dto.Remarks,
                    CurrentStatus = TrackingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.OrderTrackings.Add(orderTracking);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Order tracking created successfully", id = orderTracking.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/OrderTracking
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var trackings = await _context.OrderTrackings
                    .Include(o => o.Order)
                    .Include(u => u.User)
                    .ToListAsync();

                var result = trackings.Select(t => new OrderTrackingDto
                {
                    Id = t.Id,
                    OrderId = t.OrderId,
                    UserId = t.UserId,
                    TrackingId = t.TrackingId,
                    CurrentStatus = t.CurrentStatus.ToString(),
                    Remarks = t.Remarks,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to fetch data", error = ex.Message });
            }
        }

        // GET: api/OrderTracking/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var t = await _context.OrderTrackings.FindAsync(id);

                if (t == null)
                    return NotFound(new { success = false, message = "Tracking not found" });

                var dto = new OrderTrackingDto
                {
                    Id = t.Id,
                    OrderId = t.OrderId,
                    UserId = t.UserId,
                    TrackingId = t.TrackingId,
                    CurrentStatus = t.CurrentStatus.ToString(),
                    Remarks = t.Remarks,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                };

                return Ok(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching tracking", error = ex.Message });
            }
        }

        // PUT: api/OrderTracking/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            try
            {
                var tracking = await _context.OrderTrackings.FindAsync(id);

                if (tracking == null)
                    return NotFound(new { success = false, message = "Tracking not found" });

                if (!Enum.TryParse(newStatus, out TrackingStatus status))
                    return BadRequest(new { success = false, message = "Invalid status value" });

                tracking.CurrentStatus = status;
                tracking.UpdatedAt = DateTime.UtcNow;

                _context.Entry(tracking).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Tracking status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating status", error = ex.Message });
            }
        }

        // DELETE: api/OrderTracking/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            try
            {
                var tracking = await _context.OrderTrackings.FindAsync(id);

                if (tracking == null)
                    return NotFound(new { success = false, message = "Tracking not found" });

                tracking.IsActive = false;
                tracking.UpdatedAt = DateTime.UtcNow;

                _context.Entry(tracking).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Tracking marked as inactive" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting tracking", error = ex.Message });
            }
        }
    }
}
