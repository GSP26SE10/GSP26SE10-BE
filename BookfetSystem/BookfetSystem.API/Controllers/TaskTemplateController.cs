using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/task-template")]
    [ApiController]
    [Authorize]
    public class TaskTemplateController : ControllerBase
    {
        private readonly ITaskTemplateService _taskTemplateService;

        public TaskTemplateController(ITaskTemplateService taskTemplateService)
        {
            _taskTemplateService = taskTemplateService;
        }

        [HttpGet]
        public async Task<ActionResult> GetMyTaskTemplates([FromQuery] TaskTemplateFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var ownerIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdValue, out var ownerId))
            {
                return Unauthorized(new { Message = "Invalid token: missing owner id." });
            }

            var result = await _taskTemplateService.GetMyTaskTemplatesAsync(ownerId, filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTaskTemplate([FromBody] TaskTemplateCreateRequest request)
        {
            var ownerIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdValue, out var ownerId))
            {
                return Unauthorized(new { Message = "Invalid token: missing owner id." });
            }

            var result = await _taskTemplateService.CreateAsync(ownerId, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTaskTemplate(int id, [FromBody] TaskTemplateUpdateRequest request)
        {
            var ownerIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdValue, out var ownerId))
            {
                return Unauthorized(new { Message = "Invalid token: missing owner id." });
            }

            var result = await _taskTemplateService.UpdateAsync(id, ownerId, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTaskTemplate(int id)
        {
            var ownerIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(ownerIdValue, out var ownerId))
            {
                return Unauthorized(new { Message = "Invalid token: missing owner id." });
            }

            var result = await _taskTemplateService.DeleteAsync(id, ownerId);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
