using Ecommerce_APIs.Data;
using Ecommerce_APIs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Controllers
{

[ApiController]
[Route("api/[controller]")]
public class PincodeController : ControllerBase
    {
        private readonly PincodeService _pincodeService;
        private readonly ApplicationDbContext _db;

        public PincodeController(PincodeService pincodeService,ApplicationDbContext db)
        {
            _pincodeService = pincodeService;
            _db = db;
        }

        [HttpGet("check/{zipcode}")]
        public async Task<IActionResult> CheckPincode(string zipcode)
        {
            var allowedStates = await _db.AllowedStates.Select(s => s.Name).ToListAsync();
            var allowedCities = await _db.AllowedCities.Select(c => c.Name).ToListAsync();

            var rawData = await _pincodeService.GetRawPincodeInfoAsync(zipcode);
            if (rawData == null || rawData.Status != "Success" || rawData.PostOffice == null)
                return NotFound(new { message = "Invalid or undeliverable pincode" });

            var matchingPO = rawData.PostOffice.FirstOrDefault(po =>
                allowedStates.Contains(po.Circle, StringComparer.OrdinalIgnoreCase) ||
                allowedCities.Contains(po.District, StringComparer.OrdinalIgnoreCase));

            bool isDeliverable = matchingPO != null;
            var defaultPO = rawData.PostOffice.FirstOrDefault();

            return Ok(new
            {
                pincode = zipcode,
                deliverable = isDeliverable,
                state = (matchingPO ?? defaultPO)?.Circle,
                city = (matchingPO ?? defaultPO)?.District
            });
        }
    }

}
