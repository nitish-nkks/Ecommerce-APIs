using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.BlogPostDtos;
using Ecommerce_APIs.Models.DTOs.GlobalFilterDtos;
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
    [Authorize]
    public class BlogPostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BlogPostsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogPosts([FromQuery] GlobalFilterDto filter)
        {
            try
            {
                var query = _context.BlogPosts.AsQueryable();
                if (filter.IsActive.HasValue)
                    query = query.Where(p => p.IsActive == filter.IsActive.Value);

                var posts = await _context.BlogPosts.ToListAsync();
                var result = _mapper.Map<IEnumerable<BlogPostDto>>(posts);
                return Ok(new { success = true, message = "Blog posts fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred while fetching blog posts" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogPost(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                    return NotFound(new { success = false, message = "Blog post not found" });

                var result = _mapper.Map<BlogPostDto>(post);
                return Ok(new { success = true, message = "Blog post fetched successfully", data = result });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred while fetching blog post" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlogPost([FromBody] BlogPostCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid blog post data", data = ModelState });

                var post = _mapper.Map<BlogPost>(dto);
                post.CreatedAt = DateTime.Now;

                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();

                var result = _mapper.Map<BlogPostDto>(post);
                return Ok(new { success = true, message = "Blog post created successfully", data = result });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred while creating blog post" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlogPost(int id, [FromBody] BlogPostCreateDto dto)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                    return NotFound(new { success = false, message = "Blog post not found" });

                _mapper.Map(dto, post);
                post.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Blog post updated successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred while updating blog post" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            try
            {
                var post = await _context.BlogPosts.FindAsync(id);
                if (post == null)
                    return NotFound(new { success = false, message = "Blog post not found" });

                post.IsActive = false;
                post.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Blog post deleted successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new { success = false, message = "An error occurred while deleting blog post" });
            }
        }
    }
}
