using System.Net.Http;
using System.Threading.Tasks;
using Ecommerce_APIs.Data;
using Ecommerce_APIs.Models.DTOs.PincodeDtos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Ecommerce_APIs.Services
{  
    public class PincodeService
    {
        private readonly ApplicationDbContext _db;
        private readonly HttpClient _httpClient;

        public PincodeService(HttpClient httpClient,ApplicationDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<RawPincodeResponse?> GetRawPincodeInfoAsync(string zipcode)
        {
            var response = await _httpClient.GetAsync($"https://api.postalpincode.in/pincode/{zipcode}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<RawPincodeResponse>>(content);
            return list?.FirstOrDefault();
        }

        public async Task<PincodeCheckResponse> CheckPincodeAsync(string zipcode)
        {
            var pincodeData = await GetRawPincodeInfoAsync(zipcode);

            if (pincodeData == null || pincodeData.Status != "Success" || pincodeData.PostOffice == null || !pincodeData.PostOffice.Any())
            {
                return new PincodeCheckResponse
                {
                    Deliverable = false,
                    Message = "Invalid or undeliverable pincode."
                };
            }

            var allowedStates = await _db.AllowedStates.Select(s => s.Name).ToListAsync();
            var allowedCities = await _db.AllowedCities.Select(c => c.Name).ToListAsync();

            var postOffices = pincodeData.PostOffice;
            bool isDeliverable = postOffices.Any(po =>
                allowedStates.Contains(po.State, StringComparer.OrdinalIgnoreCase) ||
                allowedCities.Contains(po.District, StringComparer.OrdinalIgnoreCase));

            return new PincodeCheckResponse
            {
                Deliverable = isDeliverable,
                Message = isDeliverable ? "Pincode is deliverable." : "Sorry, we don't deliver to this area."
            };
        }
    }
}