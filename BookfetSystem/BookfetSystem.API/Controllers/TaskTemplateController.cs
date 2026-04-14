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
        [AllowAnonymous]
        public async Task<ActionResult> GetMyTaskTemplates([FromQuery] TaskTemplateFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _taskTemplateService.GetTaskTemplatesAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetTaskTemplateById(int id)
        {
            var result = await _taskTemplateService.GetByIdAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateTaskTemplate([FromBody] TaskTemplateCreateRequest request)
        {
            var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(roleValue, out var roleId))
            {
                return Unauthorized(new { Message = "Invalid token: missing role id." });
            }

            if (roleId != 1)
            {
                return StatusCode(403, new { Message = "Only owner can create task templates." });
            }

            var result = await _taskTemplateService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTaskTemplate(int id, [FromBody] TaskTemplateUpdateRequest request)
        {
            var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(roleValue, out var roleId))
            {
                return Unauthorized(new { Message = "Invalid token: missing role id." });
            }

            if (roleId != 1)
            {
                return StatusCode(403, new { Message = "Only owner can update task templates." });
            }

            var result = await _taskTemplateService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTaskTemplate(int id)
        {
            var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(roleValue, out var roleId))
            {
                return Unauthorized(new { Message = "Invalid token: missing role id." });
            }

            if (roleId != 1)
            {
                return StatusCode(403, new { Message = "Only owner can delete task templates." });
            }

            var result = await _taskTemplateService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            if (result.Message == "Task template not found.")
            {
                return NotFound(result);
            }

            if (result.Message == "Cannot delete task template because it is being used in related data.")
            {
                return Conflict(result);
            }

            return BadRequest(result);
        }
    }
}
