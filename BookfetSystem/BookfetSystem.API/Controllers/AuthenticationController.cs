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
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authenticationService.Register(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var result = await _authenticationService.VerifyEmail(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("resend-verification-code")]
        public async Task<ActionResult> ResendVerificationCode([FromBody] ResendVerificationRequest request)
        {
            var result = await _authenticationService.ResendVerificationCode(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
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

            // Xác định xem đây là call từ mobile (deeplink) hay web
            var isMobile = !string.IsNullOrEmpty(state) && state != "web";
            var mobileTarget = isMobile ? Uri.UnescapeDataString(state!) : null;

            // Check for errors from Google OAuth
            if (!string.IsNullOrEmpty(error))
            {
                if (isMobile && mobileTarget != null)
                {
                    return Redirect($"{mobileTarget}?error=access_denied");
                }

                return Redirect($"{frontendUrl}/login?error=access_denied");
            }

            if (string.IsNullOrEmpty(code))
            {
                if (isMobile && mobileTarget != null)
                {
                    return Redirect($"{mobileTarget}?error=no_code");
                }

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
                // - Nếu web  -> redirect về frontend web
                // - Nếu mobile -> redirect về deeplink (myapp://...)
                var target = isMobile && mobileTarget != null
                    ? mobileTarget
                    : $"{frontendUrl}/auth/callback";

                var d = result.Data;
                var query = new List<string>
                {
                    $"token={Uri.EscapeDataString(d.AccessToken ?? "")}"
                };
                if (isMobile && mobileTarget != null)
                {
                    query.Add($"userId={d.UserId}");
                    if (!string.IsNullOrEmpty(d.Email)) query.Add($"email={Uri.EscapeDataString(d.Email)}");
                    if (!string.IsNullOrEmpty(d.FullName)) query.Add($"fullName={Uri.EscapeDataString(d.FullName)}");
                    if (!string.IsNullOrEmpty(d.RoleName)) query.Add($"roleName={Uri.EscapeDataString(d.RoleName)}");
                    if (!string.IsNullOrEmpty(d.Status)) query.Add($"status={Uri.EscapeDataString(d.Status)}");
                }
                return Redirect($"{target}?{string.Join("&", query)}");
            }

            // Xử lý lỗi khi backend login thất bại
            var encodedMessage = Uri.EscapeDataString(result.Message ?? "Login failed");

            if (isMobile && mobileTarget != null)
            {
                return Redirect($"{mobileTarget}?error=login_failed&message={encodedMessage}");
            }

            return Redirect($"{frontendUrl}/login?error=login_failed&message={encodedMessage}");
        }
    }
}
