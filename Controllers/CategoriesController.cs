using System.Security.Claims;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
using Ecommerce_APIs.Models.DTOs.CatagoriesDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
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


        [HttpGet("list")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId == null ? null : c.ParentCategoryId,
                })
                .ToListAsync();

            if (categories == null || categories.Count == 0)
            {
                return NotFound("No categories found.");
            }

            return Ok(categories);
        }

        [HttpGet("tree/{id:int}")]
        public async Task<IActionResult> GetSubTree(int id)
        {
            try
            {
                var all = await _context.Categories.ToListAsync();

                List<Category> Build(int parentId) =>
                    all.Where(c => c.ParentCategoryId == parentId)
                       .Select(c => {
                           c.SubCategories = Build(c.Id);
                           return c;
                       })
                       .ToList();

                var root = all.FirstOrDefault(c => c.Id == id);
                if (root == null)
                {
                    return NotFound(new
                    {
                        Succeeded = false,
                        Message = "Category not found.",
                        Data = (string?)null
                    });
                }

                root.SubCategories = Build(root.Id);

                return Ok(new
                {
                    Succeeded = true,

                    Message = "Category subtree built successfully.",
                    Data = root
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,

                    Message = "An error occurred while building the category subtree.",
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
        public async Task<IActionResult> AddCategory([FromForm] AddCategoryDto dto)
        {
            try
            {
                int userId = TokenHelper.GetUserIdFromClaims(User);

                string normalizedNewName = dto.Name?.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(normalizedNewName))
                {
                    return BadRequest(new { Succeeded = false, Message = "Category name is required." });
                }
                bool exists = await _context.Categories
                    .AnyAsync(c => c.IsActive &&
                                   c.Name.Trim().ToLower() == normalizedNewName &&
                                   c.ParentCategoryId == dto.ParentCategoryId);
                if (exists)
                {
                    return BadRequest(new { Succeeded = false, Message = "A category with the same name already exists." });
                }

                string imageUrl = null, iconUrl = null;
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/categories");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                if (dto.Image != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Image.CopyToAsync(stream);
                    }
                    imageUrl = "/uploads/categories/" + fileName;
                }

                if (dto.Icon != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(dto.Icon.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Icon.CopyToAsync(stream);
                    }
                    iconUrl = "/uploads/categories/" + fileName;
                }

                var category = new Category
                {
                    Name = dto.Name.Trim(),
                    ParentCategoryId = dto.ParentCategoryId,
                    Image = imageUrl,
                    Icon = iconUrl,
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                    CreatedById = userId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(new { Succeeded = true, Message = "Category added successfully.", Data = category });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { Succeeded = false, Message = "An error occurred while adding the category." });
            }
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                int userId = TokenHelper.GetUserIdFromClaims(User);
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new
                    {
                        Succeeded = false,
                        Message = "Category not found.",
                        Data = (string?)null
                    });
                }

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

                bool duplicateExists = await _context.Categories
                    .AnyAsync(c => c.IsActive &&
                                   c.Id != id &&
                                   c.Name.Trim().ToLower() == normalizedNewName &&
                                   c.ParentCategoryId == dto.ParentCategoryId);

                if (duplicateExists)
                {
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "A category with the same name already exists under this parent.",
                        Data = (string?)null
                    });
                }

                category.Name = dto.Name.Trim();
                category.ParentCategoryId = dto.ParentCategoryId;
                category.Image = string.IsNullOrWhiteSpace(dto.Image) ? null : dto.Image.Trim();
                category.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
                category.Icon = string.IsNullOrWhiteSpace(dto.Icon) ? null : dto.Icon.Trim();
                category.UpdatedById = userId;
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

        [HttpGet("categories-with-products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategoriesWithProducts()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.Products)
                    .Include(c => c.Products)
                    .Select(c => new
                    {
                        CategoryId = c.Id,
                        CategoryName = c.Name,
                        Products = c.Products.Select(p => new
                        {
                            ProductId = p.Id,
                            ProductName = p.Name,
                            p.Image
                        }).ToList(),
                        SubCategories = c.SubCategories.Select(sc => new
                        {
                            SubCategoryId = sc.Id,
                            SubCategoryName = sc.Name,
                            Products = sc.Products.Select(p => new
                            {
                                ProductId = p.Id,
                                ProductName = p.Name,
                                p.Image
                            }).ToList()
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Categories with products fetched successfully.",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while fetching categories and products.",
                    Data = (string?)null
                });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                int userId = TokenHelper.GetUserIdFromClaims(User);
                var category = await _context.Categories.FindAsync(id);
                if (category == null) return NotFound(new
                {
                    Succeeded = false,
                    Message = "Category not found.",
                    Data = (string?)null
                });

                category.IsActive = false;
                category.UpdatedById = userId;
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
