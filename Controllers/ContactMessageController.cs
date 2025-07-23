using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.ContactMessageDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactMessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ContactMessageController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] ContactMessageCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var contact = _mapper.Map<ContactMessage>(dto);
                contact.IsRead = false;
                contact.CreatedAt = DateTime.UtcNow;

                _context.ContactMessages.Add(contact);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An unexpected error occurred while creating the message.");
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<ContactMessageDto>> GetMessages()
        {
            try
            {
                var messages = _context.ContactMessages
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                var dtoList = _mapper.Map<List<ContactMessageDto>>(messages);
                return Ok(dtoList);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, "An unexpected error occurred while fetching messages.");
            }
        }
    }
}
