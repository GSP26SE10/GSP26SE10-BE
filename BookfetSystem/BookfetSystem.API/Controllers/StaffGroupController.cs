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
            try
            {
                var groups = await _staffGroupService.GetAllStaffGroupFilteredAsync(filter, page, pageSize);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateStaffGroup([FromBody] StaffGroupCreateRequest request)
        {
            try
            {
                var result = await _staffGroupService.CreateAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStaffGroup(int id, [FromBody] StaffGroupUpdateRequest request)
        {
            try
            {
                var result = await _staffGroupService.UpdateAsync(id, request);
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStaffGroup(int id)
        {
            try
            {
                var result = await _staffGroupService.DeleteAsync(id);
                if (result.Success)
                {
                    return NoContent();
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

