using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookfetSystem.API.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { Message = "Invalid token: missing user id." });
            }

            var result = await _notificationService.GetAllNotificationFilteredAsync(filter, userId, page, pageSize);
            return Ok(result);
        }

        [Authorize]
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { Message = "Invalid token: missing user id." });
            }

            var result = await _notificationService.MarkAsReadAsync(id, userId);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Notification not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
    }
}
