using AutoMapper;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.UserDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using Ecommerce_APIs.Helpers;

namespace Ecommerce_APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;


        public UsersController(ApplicationDbContext dbContext,IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;

        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddUser([FromBody] AddUserDto addUserDto)
        {
            try
            {
                if (dbContext.users.Any(u => u.Email == addUserDto.Email))
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Email already exists. Please use a different email.",
                    });
                }

                var user = mapper.Map<Users>(addUserDto);
                user.PasswordHash = PasswordHasherHelper.HashPassword(addUserDto.PasswordHash);
                user.Role = addUserDto.Role ?? UserRole.Customer;
                user.IsActive = addUserDto.IsActive ?? true;
                user.CreatedAt = DateTime.Now;

                dbContext.users.Add(user);
                dbContext.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "User created successfully.",
                    data = user
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the user.",
                    data = (string?)null
                });
            }
        }

        [HttpPatch("{id:int}")]
        public IActionResult UpdateUser([FromBody] UpdateUserDto updateUserDto, int id)
        {
            try
            {
                var user = dbContext.users.Find(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found.",
                        data = (string?)null
                    });
                }

                if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
                    user.FirstName = updateUserDto.FirstName;

                if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
                    user.LastName = updateUserDto.LastName;

                if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
                {
                    var existingUser = dbContext.users.FirstOrDefault(u => u.Email == updateUserDto.Email && u.Id != id);
                    if (existingUser != null)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = "Email already exists. Please use a different email.",
                        });
                    }
                    user.Email = updateUserDto.Email;
                }


                if (!string.IsNullOrWhiteSpace(updateUserDto.PasswordHash))
                    user.PasswordHash = PasswordHasherHelper.HashPassword(updateUserDto.PasswordHash);

                user.UpdatedAt = DateTime.Now;

                dbContext.users.Update(user);
                dbContext.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "User updated successfully.",
                    data = user
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the user.",
                    data = (string?)null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = dbContext.users.FirstOrDefault(u => u.Id == id && u.IsActive);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found.",
                        data = (string?)null
                    });
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.Now;

                dbContext.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "User deleted successfully."
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the user.",
                    data = (string?)null
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                var user = dbContext.users.Find(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No user found with ID {id}.",
                        data = (string?)null
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"User with ID {id} retrieved successfully.",
                    data = user
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the user.",
                    data = (string?)null
                });
            }
        }
    }
}
