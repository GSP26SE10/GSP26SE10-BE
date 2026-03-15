using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/staff-group-member")]
    [ApiController]
    public class StaffGroupMemberController : ControllerBase
    {
        private readonly IStaffGroupMemberService _staffGroupMemberService;

        public StaffGroupMemberController(IStaffGroupMemberService staffGroupMemberService)
        {
            _staffGroupMemberService = staffGroupMemberService;
        }


        [HttpGet]
        public async Task<ActionResult> GetAllStaffGroupMembersFiltered([FromQuery] StaffGroupMemberFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var members = await _staffGroupMemberService.GetAllStaffGroupMemberFilteredAsync(filter, page, pageSize);
            return Ok(members);
        }

        [HttpPost]
        public async Task<ActionResult> CreateStaffGroupMember([FromBody] StaffGroupMemberCreateRequest request)
        {
            var result = await _staffGroupMemberService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStaffGroupMember(int id, [FromBody] StaffGroupMemberUpdateRequest request)
        {
            var result = await _staffGroupMemberService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStaffGroupMember(int id)
        {
            var result = await _staffGroupMemberService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
