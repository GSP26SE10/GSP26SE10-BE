using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BookfetSystem.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
        public class AuthenticationController : ControllerBase
        {
            private readonly IAuthenticationService _authenticationService;
            private readonly IConfiguration _configuration;

            public AuthenticationController(IAuthenticationService authenticationService, IConfiguration configuration)
            {
                _authenticationService = authenticationService;
                _configuration = configuration;
            }
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _authenticationService.Login(loginRequest);
            if (result.Success)
            {
                return Ok(result);
            }

            return Unauthorized(result);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery] string? redirect)
        {
            var googleConfig = _configuration.GetSection("Google");
            var clientId = googleConfig["ClientId"];

            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest("Google OAuth ClientId is not configured");
            }

            // Create the redirect URI for Google OAuth callback
            // Ưu tiên scheme do reverse proxy set (X-Forwarded-Proto), fallback về config ForceHttpsHosts
            var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var forceHttpsHosts = _configuration.GetSection("ForceHttpsHosts").Get<string[]>() ?? Array.Empty<string>();
            var scheme =
                !string.IsNullOrEmpty(forwardedProto) ? forwardedProto :
                forceHttpsHosts.Contains(Request.Host.Host, StringComparer.OrdinalIgnoreCase) ? "https" :
                Request.Scheme;
            var redirectUri = $"{scheme}://{Request.Host}/api/authentication/google-callback";

            // state dùng để phân biệt web/mobile và mang theo deep link (nếu có)
            var state = string.IsNullOrEmpty(redirect)
                ? "web"
                : Uri.EscapeDataString(redirect);

            // Google OAuth URL với scopes và state
            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={Uri.EscapeDataString(clientId)}&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"response_type=code&" +
                $"scope={Uri.EscapeDataString("openid email profile")}&" +
                $"access_type=offline&" +
                $"prompt=consent&" +
                $"state={state}";

            return Redirect(googleAuthUrl);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(
            [FromQuery] string? code,
            [FromQuery] string? error,
            [FromQuery] string? state)
        {
            // Get frontend URL from configuration 
            var corsOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" };
            var frontendUrl = corsOrigins.FirstOrDefault() ?? "http://localhost:3000";

            // Check for errors from Google OAuth
            if (!string.IsNullOrEmpty(error))
            {
                return Redirect($"{frontendUrl}/login?error=access_denied");
            }

            if (string.IsNullOrEmpty(code))
            {
                return Redirect($"{frontendUrl}/login?error=no_code");
            }

            // Create the redirect URI for Google OAuth callback
            // Ưu tiên scheme do reverse proxy set (X-Forwarded-Proto), fallback về config ForceHttpsHosts
            var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var forceHttpsHosts = _configuration.GetSection("ForceHttpsHosts").Get<string[]>() ?? Array.Empty<string>();
            var scheme =
                !string.IsNullOrEmpty(forwardedProto) ? forwardedProto :
                forceHttpsHosts.Contains(Request.Host.Host, StringComparer.OrdinalIgnoreCase) ? "https" :
                Request.Scheme;
            var redirectUri = $"{scheme}://{Request.Host}/api/authentication/google-callback";

            // Call the authentication service to exchange the code for a token and log in the user
            var result = await _authenticationService.LoginGoogle(code, redirectUri);

            if (result.Success && result.Data != null)
            {
                // Xác định URL đích cuối cùng:
                // - Nếu state = "web" hoặc null -> redirect về frontend web
                // - Nếu state khác "web"       -> coi là deep link (myapp://...) cho mobile
                var target = !string.IsNullOrEmpty(state) && state != "web"
                    ? Uri.UnescapeDataString(state)
                    : $"{frontendUrl}/auth/callback";

                return Redirect($"{target}?token={Uri.EscapeDataString(result.Data.AccessToken)}");
            }

            return Redirect($"{frontendUrl}/login?error=login_failed&message={Uri.EscapeDataString(result.Message ?? "Login failed")}");
        }
    }
}
