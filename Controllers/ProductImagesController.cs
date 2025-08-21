using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.ProductImageDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductImagesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadProductImage([FromForm] ProductImageCreateDto dto)
        {
            try
            {
                if (dto == null || dto.Image == null || dto.ProductId <= 0)
                    return BadRequest(new { Succeeded = false, Message = "Invalid payload", Data = (object?)null });

                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                    return NotFound(new { Succeeded = false, Message = "Product not found", Data = (object?)null });

                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(dto.Image.FileName);
                var uniqueFileName = Guid.NewGuid().ToString("N") + ext;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/products/{uniqueFileName}";

                if (dto.IsPrimary)
                {
                    var existingPrimary = await _context.ProductImages
                        .Where(pi => pi.ProductId == dto.ProductId && pi.IsActive && pi.IsPrimary)
                        .ToListAsync();

                    foreach (var ex in existingPrimary)
                    {
                        ex.IsPrimary = false;
                        ex.UpdatedAt = DateTime.Now;
                    }
                }

                var productImage = new ProductImage
                {
                    ProductId = dto.ProductId,
                    ImageUrl = imageUrl,
                    IsPrimary = dto.IsPrimary,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();

                var resultDto = new ProductImageDto
                {
                    Id = productImage.Id,
                    ImageUrl = productImage.ImageUrl,
                    IsPrimary = productImage.IsPrimary,
                    CreatedAt = productImage.CreatedAt
                };

                return Ok(new { Succeeded = true, Message = "Image uploaded", Data = resultDto });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { Succeeded = false, Message = "An error occurred while uploading the product image.", Data = (object?)null });
            }
        }

        [HttpGet("{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImagesForProduct(int productId)
        {
            try
            {
                var images = await _context.ProductImages
                    .Where(img => img.ProductId == productId && img.IsActive)
                    .OrderByDescending(img => img.IsPrimary) // primary first
                    .Select(img => new ProductImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        IsPrimary = img.IsPrimary,
                        CreatedAt = img.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { Succeeded = true, Message = "Images retrieved", Data = images });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { Succeeded = false, Message = "An error occurred while retrieving product images.", Data = (object?)null });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var image = await _context.ProductImages.FindAsync(id);
                if (image == null) return NotFound(new { Succeeded = false, Message = "Image not found", Data = (object?)null });

                if (!string.IsNullOrWhiteSpace(image.ImageUrl))
                {
                    var relativePath = image.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var filePath = Path.Combine(webRootPath, relativePath);

                    if (System.IO.File.Exists(filePath))
                    {
                        try { System.IO.File.Delete(filePath); }
                        catch
                        {
                            SentrySdk.CaptureMessage($"Failed to delete file {filePath}");
                        }
                    }
                }

                image.IsActive = false;
                image.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { Succeeded = true, Message = "Product image deleted successfully", Data = (object?)null });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { Succeeded = false, Message = "An error occurred while deleting the product image.", Data = (object?)null });
            }
        }

        [HttpPut("set-primary/{id}")]
        public async Task<IActionResult> SetPrimary(int id)
        {
            try
            {
                var image = await _context.ProductImages.FindAsync(id);
                if (image == null) return NotFound(new { Succeeded = false, Message = "Image not found", Data = (object?)null });

                var existing = await _context.ProductImages
                    .Where(i => i.ProductId == image.ProductId && i.IsActive && i.IsPrimary)
                    .ToListAsync();

                foreach (var img in existing)
                {
                    if (img.Id == image.Id) continue;
                    img.IsPrimary = false;
                    img.UpdatedAt = DateTime.Now;
                }

                image.IsPrimary = true;
                image.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var dto = new ProductImageDto
                {
                    Id = image.Id,
                    ImageUrl = image.ImageUrl,
                    IsPrimary = image.IsPrimary,
                    CreatedAt = image.CreatedAt
                };

                return Ok(new { Succeeded = true, Message = "Primary image set", Data = dto });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { Succeeded = false, Message = "An error occurred while setting primary image.", Data = (object?)null });
            }
        }
    }
}
