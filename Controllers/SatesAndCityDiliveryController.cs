using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SatesAndCityDiliveryController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public SatesAndCityDiliveryController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("states")]
        public IActionResult GetStates()
        {
            var states = dbContext.AllowedStates.ToList();
            if (states == null || !states.Any())
            {
                return NotFound(new { message = "No states found." });
            }
            return Ok(states);
        }
        // POST: api/states
        [HttpPost("states")]
        public IActionResult AddState([FromBody] AllowedState state)
        {
            if (string.IsNullOrWhiteSpace(state.Name))
                return BadRequest(new { message = "State name cannot be empty." });

            state.Id = 0;
            dbContext.AllowedStates.Add(state);
            dbContext.SaveChanges();
            return Ok(new { message = "State added successfully.", state });
        }

        // DELETE: api/states/{id}
        [HttpDelete("states/{id}")]
        public IActionResult DeleteState(int id)
        {
            var state = dbContext.AllowedStates.Find(id);
            if (state == null)
                return NotFound(new { message = "State not found." });

            dbContext.AllowedStates.Remove(state);
            dbContext.SaveChanges();
            return Ok(new { message = "State deleted successfully." });
        }

        [HttpGet("cities")]
        public IActionResult GetCities()
        {
            var cities = dbContext.AllowedCities.ToList();
            if (cities == null || !cities.Any())
            {
                return NotFound(new { message = "No cities found." });
            }
            return Ok(cities);
        }
        // POST: api/cities
        [HttpPost("cities")]
        public IActionResult AddCity([FromBody] AllowedCity city)
        {
            if (string.IsNullOrWhiteSpace(city.Name))
                return BadRequest(new { message = "City name cannot be empty." });

            city.Id = 0;
            dbContext.AllowedCities.Add(city);
            dbContext.SaveChanges();
            return Ok(new { message = "City added successfully.", city });
        }

        // DELETE: api/cities/{id}
        [HttpDelete("cities/{id}")]
        public IActionResult DeleteCity(int id)
        {
            var city = dbContext.AllowedCities.Find(id);
            if (city == null)
                return NotFound(new { message = "City not found." });

            dbContext.AllowedCities.Remove(city);
            dbContext.SaveChanges();
            return Ok(new { message = "City deleted successfully." });
        }


    }
}
