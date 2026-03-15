using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/staff-group")]
    [ApiController]
    public class StaffGroupController : ControllerBase
    {
        private readonly IStaffGroupService _staffGroupService;

        public StaffGroupController(IStaffGroupService staffGroupService)
        {
            _staffGroupService = staffGroupService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllStaffGroupsFiltered([FromQuery] StaffGroupFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var groups = await _staffGroupService.GetAllStaffGroupFilteredAsync(filter, page, pageSize);
            return Ok(groups);
        }

        [HttpPost]
        public async Task<ActionResult> CreateStaffGroup([FromBody] StaffGroupCreateRequest request)
        {
            var result = await _staffGroupService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStaffGroup(int id, [FromBody] StaffGroupUpdateRequest request)
        {
            var result = await _staffGroupService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStaffGroup(int id)
        {
            var result = await _staffGroupService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }

        [Authorize]
        [HttpGet("leader/orders-overview")]
        public async Task<ActionResult> GetMyAssignmentOverview()
        {
            var leaderIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(leaderIdValue, out var leaderId))
            {
                return Unauthorized(new { Message = "Invalid token: missing leader id." });
            }

            var result = await _staffGroupService.GetAssignmentOverviewByLeaderAsync(leaderId);
            if (result == null)
            {
                return NotFound(new { Message = "Leader does not have a staff group." });
            }

            return Ok(result);
        }
    }
}

