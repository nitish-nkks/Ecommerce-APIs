using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.UserDtos;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Ecommerce_APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public UsersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        public IActionResult AddUser([FromBody] AddUserDto addUserDto)
        {
            try
            {
                var user = new Users
                {
                    FirstName = addUserDto.FirstName,
                    LastName = addUserDto.LastName,
                    Email = addUserDto.Email,
                    PasswordHash = addUserDto.PasswordHash,
                    Role = addUserDto.Role ?? UserRole.Customer,
                    IsActive = addUserDto.IsActive ?? true,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.userss.Add(user);
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
                var user = dbContext.userss.Find(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found.",
                        data = (string?)null
                    });
                }

                if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName)) user.FirstName = updateUserDto.FirstName;
                if (!string.IsNullOrWhiteSpace(updateUserDto.LastName)) user.LastName = updateUserDto.LastName;
                if (!string.IsNullOrWhiteSpace(updateUserDto.Email)) user.Email = updateUserDto.Email;
                if (!string.IsNullOrWhiteSpace(updateUserDto.PasswordHash)) user.PasswordHash = updateUserDto.PasswordHash;

                user.UpdatedAt = DateTime.UtcNow;

                dbContext.userss.Update(user);
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
                var user = dbContext.userss.Find(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found.",
                        data = (string?)null
                    });
                }

                dbContext.userss.Remove(user);
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
                var user = dbContext.userss.Find(id);
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
