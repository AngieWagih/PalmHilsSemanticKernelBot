using Microsoft.AspNetCore.Mvc;
using PalmHilsSemanticKernelBot.Controllers.Response;
using System.Text.Json;

namespace PalmHilsSemanticKernelBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DirectLineController:ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DirectLineController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken()
        {
            try
            {
                var directLineSecret = _configuration["DirectLineSecret"];

                if (string.IsNullOrEmpty(directLineSecret))
                {
                    
                    return BadRequest(new { error = "DirectLine not configured" });
                }

                using var httpClient = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Post, "https://directline.botframework.com/v3/directline/tokens/generate");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", directLineSecret);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<DirectLineTokenResponse>(content);

                    return Ok(new { token = tokenResponse.token });
                }
                else
                {
                   
                    return StatusCode(500, new { error = "Failed to get DirectLine token" });
                }
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
