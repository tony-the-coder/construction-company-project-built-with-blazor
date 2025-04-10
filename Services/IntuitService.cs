using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LehmanCustomConstruction.Services 
{
    public class IntuitService
    {
        private readonly HttpClient _httpClient;

        public IntuitService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetCompanyInfoAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                // Replace YOUR_COMPANY_ID with your actual QuickBooks company ID
                var response = await _httpClient.GetAsync("/v3/company/YOUR_COMPANY_ID/companyinfo/YOUR_COMPANY_ID");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine($"API request failed: {ex.Message}");
                return null; // Or throw the exception, depending on your error handling
            }
        }

        // Add other Intuit API methods here as needed
    }
}