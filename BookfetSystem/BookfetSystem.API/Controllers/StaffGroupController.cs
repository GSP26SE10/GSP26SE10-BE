using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System;
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
    }
}

