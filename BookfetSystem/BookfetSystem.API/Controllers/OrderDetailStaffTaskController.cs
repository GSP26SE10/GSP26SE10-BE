using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order-detail-staff-task")]
    [ApiController]
    public class OrderDetailStaffTaskController : ControllerBase
    {
        private readonly IOrderDetailStaffTaskService _orderDetailStaffTaskService;

        public OrderDetailStaffTaskController(IOrderDetailStaffTaskService orderDetailStaffTaskService)
        {
            _orderDetailStaffTaskService = orderDetailStaffTaskService;
        }

        [Authorize]
        [HttpGet("staff-tasks")]
        public async Task<ActionResult> GetMyTasks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var staffIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(staffIdValue, out var staffId))
            {
                return Unauthorized(new { Message = "Invalid token: missing staff id." });
            }

            var result = await _orderDetailStaffTaskService.GetMyTasksAsync(staffId, page, pageSize);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllOrderDetailStaffTasksFiltered([FromQuery] OrderDetailStaffTaskFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var tasks = await _orderDetailStaffTaskService.GetAllOrderDetailStaffTaskFilteredAsync(filter, page, pageSize);
            return Ok(tasks);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateOrderDetailStaffTask([FromBody] OrderDetailStaffTaskCreateRequest request)
        {
            var leaderIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(leaderIdValue, out var leaderId))
            {
                return Unauthorized(new { Message = "Invalid token: missing leader id." });
            }

            var result = await _orderDetailStaffTaskService.CreateAsync(request, leaderId);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpPatch("{id}/accept")]
        public async Task<ActionResult> AcceptMyTask(int id)
        {
            var staffIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(staffIdValue, out var staffId))
            {
                return Unauthorized(new { Message = "Invalid token: missing staff id." });
            }

            var result = await _orderDetailStaffTaskService.AcceptMyTaskAsync(id, staffId);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Task not found." || result.Message == "Không tìm thấy công việc.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpPatch("{id}/complete")]
        public async Task<ActionResult> CompleteMyTask(int id, [FromForm] StaffCompleteTaskRequest request)
        {
            var staffIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(staffIdValue, out var staffId))
            {
                return Unauthorized(new { Message = "Invalid token: missing staff id." });
            }

            var result = await _orderDetailStaffTaskService.CompleteMyTaskAsync(id, staffId, request);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Task not found." || result.Message == "Không tìm thấy công việc.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpPatch("{id}/staff-task-status")]
        public async Task<ActionResult> UpdateMyTaskStatus(int id, [FromBody] StaffUpdateTaskStatusRequest request)
        {
            var staffIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(staffIdValue, out var staffId))
            {
                return Unauthorized(new { Message = "Invalid token: missing staff id." });
            }

            var result = await _orderDetailStaffTaskService.UpdateMyTaskStatusAsync(id, staffId, request);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Task not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetailStaffTask(int id, [FromBody] OrderDetailStaffTaskUpdateRequest request)
        {
            var result = await _orderDetailStaffTaskService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetailStaffTask(int id)
        {
            var result = await _orderDetailStaffTaskService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
