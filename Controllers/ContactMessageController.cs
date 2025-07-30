using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.ContactMessageDtos;
using Ecommerce_APIs.Models.Entites;
using Ecommerce_APIs.Services;
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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ContactMessageController(ApplicationDbContext context, IMapper mapper,IEmailService emailService,IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _config = config;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateMessage([FromBody] ContactMessageCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "Invalid contact message.",
                        Data = (string?)null
                    });

                var contact = _mapper.Map<ContactMessage>(dto);
                contact.IsRead = false;
                contact.IsActive = true;
                contact.CreatedAt = DateTime.Now;

                _context.ContactMessages.Add(contact);
                await _context.SaveChangesAsync();

                var contactEmail = _config["Contact:RecipientEmail"] ?? "example@gmail.com";
                await _emailService.SendContactNotificationEmail(contact, contactEmail);

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Message sent successfully.",
                    Data = _mapper.Map<ContactMessageDto>(contact)
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An unexpected error occurred while creating the message.",
                    Data = (string?)null
                });
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
                return Ok(new
                {
                    Succeeded = true,
                    Message = "Messages fetched successfully.",
                    Data = dtoList
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    Succeeded = false,
                    Message = "An unexpected error occurred while fetching messages.",
                    Data = (string?)null
                });
            }
        }
    }
}
