using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Helpers;
using Ecommerce_APIs.Models.DTOs.InternalUserDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InternalUserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public InternalUserController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateInternalUser([FromBody] CreateInternalUserDto dto)
        {
            try
            {
                if (await _context.InternalUsers.AnyAsync(u => u.Email == dto.Email))
                {
                    return BadRequest(new
                    {
                        Succeeded = false,
                        Message = "Email already exists.",
                        Data = (object?)null
                    });
                }

                var user = _mapper.Map<InternalUser>(dto);
                user.PasswordHash = PasswordHasherHelper.HashPassword(dto.PasswordHash);
                user.IsActive = true;
                user.CreatedAt = DateTime.Now;

                _context.InternalUsers.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Internal user created successfully.",
                    Data = new { user.Id }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Succeeded = false,
                    Message = $"An error occurred while creating user: {ex.Message}",
                    Data = (object?)null
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInternalUser(int id, [FromBody] UpdateInternalUserDto dto)
        {
            try
            {
                var user = await _context.InternalUsers.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Succeeded = false,
                        Message = "User not found.",
                        Data = (object?)null
                    });
                }

                _mapper.Map(dto, user);

                if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                {
                    user.PasswordHash = PasswordHasherHelper.HashPassword(dto.PasswordHash);
                }

                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Internal user updated successfully.",
                    Data = new { user.Id }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Succeeded = false,
                    Message = $"An error occurred while updating user: {ex.Message}",
                    Data = (object?)null
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> InActiveInternalUser(int id)
        {
            try
            {
                var user = await _context.InternalUsers.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Succeeded = false,
                        Message = "User not found.",
                        Data = (object?)null
                    });
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Succeeded = true,
                    Message = "Internal user deactivated successfully.",
                    Data = new { user.Id }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Succeeded = false,
                    Message = $"An error occurred while deactivating user: {ex.Message}",
                    Data = (object?)null
                });
            }
        }
    }
}
