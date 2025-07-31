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
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;


        public CustomerController(ApplicationDbContext dbContext,IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;

        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddCustomer([FromBody] AddCustomerDto addCustomerDto)
        {
            try
            {
                if (dbContext.Customers.Any(u => u.Email == addCustomerDto.Email))
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Email already exists. Please use a different email.",
                    });
                }

                var user = mapper.Map<Customer>(addCustomerDto);
                user.PasswordHash = PasswordHasherHelper.HashPassword(addCustomerDto.PasswordHash);

                dbContext.Customers.Add(user);
                dbContext.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Customer created successfully.",
                    data = user
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the Customer.",
                    data = (string?)null
                });
            }
        }

        [HttpPatch("{id:int}")]
        public IActionResult UpdateCustomer([FromBody] UpdateCustomerDto updateCustomerDto, int id)
        {
            try
            {
                var user = dbContext.Customers.Find(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found.",
                        data = (string?)null
                    });
                }

                if (!string.IsNullOrWhiteSpace(updateCustomerDto.FirstName))
                    user.FirstName = updateCustomerDto.FirstName;

                if (!string.IsNullOrWhiteSpace(updateCustomerDto.LastName))
                    user.LastName = updateCustomerDto.LastName;

                if (!string.IsNullOrWhiteSpace(updateCustomerDto.Email))
                {
                    var existingUser = dbContext.Customers.FirstOrDefault(u => u.Email == updateCustomerDto.Email && u.Id != id);
                    if (existingUser != null)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = "Email already exists. Please use a different email.",
                        });
                    }
                    user.Email = updateCustomerDto.Email;
                }


                if (!string.IsNullOrWhiteSpace(updateCustomerDto.PasswordHash))
                    user.PasswordHash = PasswordHasherHelper.HashPassword(updateCustomerDto.PasswordHash);

                user.UpdatedAt = DateTime.Now;

                dbContext.Customers.Update(user);
                dbContext.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Customer updated successfully.",
                    data = user
                });
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the Customer.",
                    data = (string?)null
                });
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteCustomer(int id)
        {
            try
            {
                var user = dbContext.Customers.FirstOrDefault(u => u.Id == id && u.IsActive);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Customer not found.",
                        data = (string?)null
                    });
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.Now;

                dbContext.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Customer deleted successfully."
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

        [HttpGet("{id:int}")]
        public IActionResult GetCustomerById(int id)
        {
            try
            {
                var user = dbContext.Customers.Find(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"No Customer found with ID {id}.",
                        data = (string?)null
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Customer with ID {id} retrieved successfully.",
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
