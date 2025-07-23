using System;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.ProductImageDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentry;

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
            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "products");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/products/{uniqueFileName}";

            var productImage = new ProductImage
            {
                ProductId = dto.ProductId,
                ImageUrl = imageUrl,
                IsPrimary = dto.IsPrimary,
                CreatedAt = DateTime.Now
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return Ok(productImage);
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return StatusCode(500, "An error occurred while uploading the product image.");
        }
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetImagesForProduct(int productId)
    {
        try
        {
            var images = await _context.ProductImages
                .Where(img => img.ProductId == productId)
                .Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    IsPrimary = img.IsPrimary,
                    CreatedAt = img.CreatedAt
                })
                .ToListAsync();

            return Ok(images);
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return StatusCode(500, "An error occurred while retrieving product images.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        try
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath, image.ImageUrl);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex);
            return StatusCode(500, "An error occurred while deleting the product image.");
        }
    }
}
