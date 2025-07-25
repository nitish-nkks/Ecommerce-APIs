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
        public IActionResult AddUser([FromBody] AddUserDto addUserDto)
        {
            try
            {
                var user = mapper.Map<Users>(addUserDto);
                user.PasswordHash = PasswordHasherHelper.HashPassword(addUserDto.PasswordHash);
                user.Role = addUserDto.Role ?? UserRole.Customer;
                user.IsActive = addUserDto.IsActive ?? true;
                user.CreatedAt = DateTime.Now;

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

        [Authorize]
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

                mapper.Map(updateUserDto, user); 
                user.UpdatedAt = DateTime.Now;

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
