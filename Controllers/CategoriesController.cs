using System.Security.Claims;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.CatagoriesDtos;
using Ecommerce_APIs.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("UserId") ??
                              User.FindFirst(ClaimTypes.NameIdentifier) ??
                              User.FindFirst("sub");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.SubCategories)
                    .ToListAsync();

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Categories fetched successfully.",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while fetching categories.",
                    Data = (string?)null
                });
            }
        }

        
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
                return Ok(new
                {
                    Succeeded = true,
                    Message = "Category tree built successfully.",
                    Data = tree
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while building the category tree.",
                    Data = (string?)null
                });
            }
        }

        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Include(c => c.SubCategories)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null) return NotFound(new
                {
                    Succeeded = false,
                    Message = "Category not found.",
                    Data = (string?)null
                });
                return Ok(new
                {
                    Succeeded = true,
                    Message = "Category fetched successfully.",
                    Data = category
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while fetching the category.",
                    Data = (string?)null
                });
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> AddCategory(AddCategoryDto dto)
        {
            try
            {
                string normalizedNewName = dto.Name?.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(normalizedNewName))
                {
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "Category name is required.",
                        Data = (string?)null
                    });
                }
                bool exists = await _context.Categories
                     .AnyAsync(c => c.IsActive && c.Name.Trim().ToLower() == normalizedNewName);
                if (exists)
                {
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "A category with the same name already exists.",
                        Data = (string?)null
                    });
                }
                var category = new Category
                {
                    Name = dto.Name.Trim(),
                    ParentCategoryId = dto.ParentCategoryId,
                    CreatedById = GetUserIdFromToken(),
                    CreatedAt = DateTime.Now
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Category added successfully.",
                    Data = category
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while adding the category.",
                    Data = (string?)null
                });
            }
        }

        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound(new
                {
                    Succeeded = false,
                    Message = "Category not found.",
                    Data = (string?)null
                });

                if (dto.ParentCategoryId.HasValue &&
                    !await _context.Categories.AnyAsync(c => c.Id == dto.ParentCategoryId.Value))
                {
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "Invalid ParentCategoryId.",
                        Data = (string?)null
                    });
                }

                category.Name = dto.Name;
                category.ParentCategoryId = dto.ParentCategoryId;
                category.UpdatedById = dto.UpdatedById;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Succeeded = true,
                    Message = "Category updated successfully.",
                    Data = category
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while updating the category.",
                    Data = (string?)null
                });
            }
        }

        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound(new
                {
                    Succeeded = false,
                    Message = "Category not found.",
                    Data = (string?)null
                });

                category.IsActive = false;
                category.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    Succeeded = true,
                    Message = "Category deleted successfully.",
                    Data = (string?)null
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while deleting the category.",
                    Data = (string?)null
                });
            }
        }
    }
}
