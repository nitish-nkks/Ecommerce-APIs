using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.StaticPageDtos;
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
    public class StaticPagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public StaticPagesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaticPageDto>>> GetStaticPages()
        {
            try
            {
                var pages = await _context.StaticPages.ToListAsync();
                return Ok(new { success = true, data = _mapper.Map<IEnumerable<StaticPageDto>>(pages) });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Error retrieving static pages", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StaticPageDto>> GetStaticPage(int id)
        {
            try
            {
                var page = await _context.StaticPages.FindAsync(id);
                if (page == null)
                    return NotFound(new { success = false, message = "Static page not found" });

                return Ok(new { success = true, data = _mapper.Map<StaticPageDto>(page) });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Error retrieving static page", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<StaticPageDto>> CreateStaticPage(StaticPageCreateDto dto)
        {
            try
            {
                var page = _mapper.Map<StaticPage>(dto);
                page.CreatedAt = DateTime.Now;
                _context.StaticPages.Add(page);
                await _context.SaveChangesAsync();

                var result = _mapper.Map<StaticPageDto>(page);
                return CreatedAtAction(nameof(GetStaticPage), new { id = page.Id }, new { success = true, data = result });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Error creating static page", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaticPage(int id, StaticPageUpdateDto dto)
        {
            try
            {
                var page = await _context.StaticPages.FindAsync(id);
                if (page == null)
                    return NotFound(new { success = false, message = "Static page not found" });

                _mapper.Map(dto, page);
                page.UpdatedAt = DateTime.Now;

                _context.Entry(page).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Static page updated successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Error updating static page", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaticPage(int id)
        {
            try
            {
                var page = await _context.StaticPages.FindAsync(id);
                if (page == null)
                    return NotFound(new { success = false, message = "Static page not found" });

                _context.StaticPages.Remove(page);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Static page deleted successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Error deleting static page", error = ex.Message });
            }
        }
    }
}
