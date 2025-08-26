using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
using Ecommerce_APIs.Helpers.Extensions;
using Ecommerce_APIs.Models.DTOs.GlobalFilterDtos;
using Ecommerce_APIs.Models.DTOs.ProductDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper mapper;

        public ProductsController(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(AddProductDto dto)
        {
            try
            {
                int userId = TokenHelper.GetUserIdFromClaims(User);

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid model", data = ModelState });

                string normalizedProductName = dto.Name?.Trim().ToLower();
                if (string.IsNullOrWhiteSpace(normalizedProductName))
                {
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "Product name is required.",
                        Data = (string?)null

                    });
                }

                bool exists = await _context.Products
                           .AnyAsync(p => p.IsActive &&
                           p.Name.Trim().ToLower() == normalizedProductName &&
                           p.CategoryId == dto.CategoryId);

                if (exists)
                {
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "A product with the same name already exists.",
                        Data = (string?)null
                    });
                }

                if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                    return BadRequest(new { success = false, message = "Invalid category ID" , Data = (string?)null });

                var product = mapper.Map<Product>(dto);
                if (dto.ProductImages != null && dto.ProductImages.Any())
                {
                    product.Image = string.Join(",", dto.ProductImages);
                }
                else
                {
                    product.Image = string.Empty;
                }

                product.Name = dto.Name.Trim();
                product.CreatedBy = userId;
                product.CreatedAt = DateTime.Now;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Product added", data = new { product.Id } });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred while adding the product.", data = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] GlobalFilterDto filter)
        {
            try
            {
                filter ??= new GlobalFilterDto();

                var (pagedProducts, totalCount) = await _context.Products
                    .Where(p => p.IsActive)
                    .Include(p => p.Category)
                    .ApplyGlobalFilter(filter, x => x.Name, x => x.IsActive)
                    .ToPagedListAsync(filter);

                var result = mapper.Map<IEnumerable<ProductDto>>(pagedProducts);

                return Ok(new
                {
                    success = true,
                    message = "Products fetched successfully",
                    totalCount,
                    page = filter.Page,
                    pageSize = filter.PageSize,
                    data = result
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred", data = ex.Message });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                    return NotFound(new { success = false, message = "Product not found" });

                return Ok(new { success = true, message = "Product fetched", data = product });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred", data = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found" });

                _context.Entry(product).Property(p => p.Name).IsModified = false;

                if (dto.Description != null) product.Description = dto.Description;
                if (dto.Price.HasValue)
                {
                    if (dto.Price < 0) return BadRequest(new { success = false, message = "Price cannot be negative" });
                    product.Price = dto.Price.Value;
                }
                if (dto.StockQuantity.HasValue)
                {
                    if (dto.StockQuantity < 0) return BadRequest(new { success = false, message = "Stock quantity cannot be negative" });
                    product.StockQuantity = dto.StockQuantity.Value;
                }
                if (dto.DiscountPercentage.HasValue)
                {
                    if (dto.DiscountPercentage < 0 || dto.DiscountPercentage > 100)
                        return BadRequest(new { success = false, message = "Discount must be between 0 and 100" });
                    product.DiscountPercentage = dto.DiscountPercentage.Value;
                }
                if (dto.MinOrderQuantity.HasValue) product.MinOrderQuantity = dto.MinOrderQuantity.Value;
                if (dto.IsFeatured.HasValue) product.IsFeatured = dto.IsFeatured.Value;
                if (dto.IsNewProduct.HasValue) product.IsNewProduct = dto.IsNewProduct.Value;
                if (dto.IsBestSeller.HasValue) product.IsBestSeller = dto.IsBestSeller.Value;

                if (dto.ProductImages != null)
                    product.Image = dto.ProductImages.Any()
                                    ? string.Join(",", dto.ProductImages)
                                    : product.Image;

                product.UpdatedBy = TokenHelper.GetUserIdFromClaims(User);
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "Internal server error", data = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found" });

                product.IsActive = false;
                product.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Product deleted" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred", data = ex.Message });
            }
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.IsActive)
                    .ToListAsync();

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Products fetched successfully.",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An error occurred while fetching products.",
                    Data = (string?)null
                });
            }
        }

    }
}

