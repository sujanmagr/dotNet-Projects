using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FoodY.Models;
// Ensure you have Newtonsoft.Json installed

namespace FoodY.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public PaymentController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        [HttpPost("khalti")]
        public async Task<IActionResult> InitiateKhaltiPayment([FromBody] KhaltiPaymentRequest payload)
        {
            try
            {
                // Convert payload to JSON
                var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.Indented);
                Console.WriteLine($"Payload to Khalti:\n{jsonPayload}");

                // Create a request message to the Khalti API
                var url = "https://a.khalti.com/api/v2/epayment/initiate/";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
                };
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Key", "db26d03e44be4ab2a75ea5ae03b4b285");

                // Send request to Khalti API
                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var khaltiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var paymentUrl = (string)khaltiResponse.payment_url;

                    if (!string.IsNullOrEmpty(paymentUrl))
                    {
                        // Return the payment URL to the client
                        return Ok(new { paymentUrl });
                    }
                    else
                    {
                        return BadRequest("Payment URL not provided in the Khalti response.");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error from Khalti API: {errorContent}");

                    // Return the specific error from Khalti
                    return StatusCode((int)response.StatusCode, errorContent);
                }
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors
                Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                return BadRequest("Invalid payload format.");
            }
            catch (Exception ex)
            {
                // General error handler
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while processing the request.");
            }
        }





    }
}
