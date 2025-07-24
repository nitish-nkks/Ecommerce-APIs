using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.CatagoriesDtos;
using Ecommerce_APIs.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.SubCategories)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An error occurred while fetching categories.");
            }
        }

        // GET: api/categories/tree
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            try
            {
                var all = await _context.Categories.ToListAsync();
                List<Category> Build(int? parentId) =>
                    all.Where(c => c.ParentCategoryId == parentId)
                       .Select(c => {
                           c.SubCategories = Build(c.Id);
                           return c;
                       })
                       .ToList();

                var tree = Build(null);
                return Ok(tree);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An error occurred while building the category tree.");
            }
        }

        // GET: api/categories/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.SubCategories)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null) return NotFound("Category not found");
                return Ok(category);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An error occurred while fetching the category.");
            }
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> AddCategory(AddCategoryDto dto)
        {
            try
            {
                var category = new Category
                {
                    Name = dto.Name,
                    ParentCategoryId = dto.ParentCategoryId,
                    CreatedById = dto.CreatedById,
                    CreatedAt = DateTime.Now
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(category);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An error occurred while adding the category.");
            }
        }

        // PUT: api/categories/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound("Category not found");

                if (dto.ParentCategoryId.HasValue &&
                    !await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId.Value))
                {
                    return BadRequest("Invalid ParentCategoryId.");
                }

                category.Name = dto.Name;
                category.ParentCategoryId = dto.ParentCategoryId;
                category.UpdatedById = dto.UpdatedById;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An error occurred while updating the category.");
            }
        }

        // DELETE: api/categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound("Category not found");

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An error occurred while deleting the category.");
            }
        }
    }
}
