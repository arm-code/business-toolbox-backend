using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BusinessToolbox.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var supabaseUrl = _configuration["Supabase:Url"];
        var anonKey = _configuration["Supabase:AnonKey"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(anonKey))
        {
            return StatusCode(500, new { Message = "Falta configurar Supabase:Url o Supabase:AnonKey en el appsettings." });
        }

        var client = _httpClientFactory.CreateClient();
        var url = $"{supabaseUrl}/auth/v1/token?grant_type=password";

        var payload = new
        {
            email = request.Email,
            password = request.Password
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        
        // Supabase requiere el Anon Key en los headers para peticiones públicas de Auth
        client.DefaultRequestHeaders.Add("apikey", anonKey);

        var response = await client.PostAsync(url, content);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(new { Message = "Error al autenticar", Details = responseContent });
        }

        // Retornamos directamente el JSON que nos da Supabase (que incluye el access_token)
        return Content(responseContent, "application/json");
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
