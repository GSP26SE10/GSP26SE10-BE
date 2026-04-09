using System.Security.Claims;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [Route("api/devices")]
    [ApiController]
    [Authorize]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] DeviceRegisterRequest request)
        {
            var tokenUserId = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserId, out var currentUserId))
            {
                return Unauthorized(new { Message = "Invalid token: missing user id." });
            }

            // Always trust authenticated user from JWT to avoid account-mismatch issues on shared devices.
            request.UserId = currentUserId;

            var result = await _deviceService.RegisterAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("deactivate")]
        public async Task<ActionResult> Deactivate([FromBody] DeviceDeactivateRequest request)
        {
            var result = await _deviceService.DeactivateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}