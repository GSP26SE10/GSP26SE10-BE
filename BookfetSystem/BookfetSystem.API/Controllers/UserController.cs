using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookfetSystem.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        public UserController(IUserService userService, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsersFiltered([FromQuery] UserFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var accounts = await _userService.GetAllUserFilteredAsync(filter, page, pageSize);
            return Ok(accounts);
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            var result = await _userService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            var result = await _userService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }

        [Authorize]
        [HttpPost("change-password/send-otp")]
        public async Task<ActionResult> RequestChangePasswordOtp([FromBody] ChangePasswordRequest request)
        {
            var userIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { Message = "Invalid token: missing user id." });
            }

            var result = await _authenticationService.RequestChangePasswordOtp(userId, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpPost("change-password/verify-otp")]
        public async Task<ActionResult> VerifyChangePasswordOtp([FromBody] VerifyChangePasswordOtpRequest request)
        {
            var userIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { Message = "Invalid token: missing user id." });
            }

            var result = await _authenticationService.VerifyChangePasswordOtp(userId, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
