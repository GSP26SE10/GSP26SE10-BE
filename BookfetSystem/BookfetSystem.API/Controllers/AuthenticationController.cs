using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
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
    }
}
