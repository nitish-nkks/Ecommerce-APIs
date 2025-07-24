using System.Net.Http;
using System.Threading.Tasks;
using Ecommerce_APIs.Models.DTOs.PincodeDtos;
using Newtonsoft.Json;

namespace Ecommerce_APIs.Services
{  
    public class PincodeService
    {
        private readonly HttpClient _httpClient;

        public PincodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RawPincodeResponse?> GetRawPincodeInfoAsync(string zipcode)
        {
            var response = await _httpClient.GetAsync($"https://api.postalpincode.in/pincode/{zipcode}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<RawPincodeResponse>>(content);
            return list?.FirstOrDefault();
        }

    }

}
