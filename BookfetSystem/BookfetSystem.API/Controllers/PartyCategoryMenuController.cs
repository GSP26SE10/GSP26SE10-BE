using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/party-category-menu")]
    [ApiController]
    public class PartyCategoryMenuController : ControllerBase
    {
        private readonly IPartyCategoryMenuService _partyCategoryMenuService;

        public PartyCategoryMenuController(IPartyCategoryMenuService partyCategoryMenuService)
        {
            _partyCategoryMenuService = partyCategoryMenuService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPartyCategoryMenusFiltered([FromQuery] PartyCategoryMenuFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var partyCategoryMenus = await _partyCategoryMenuService.GetAllPartyCategoryMenuFilteredAsync(filter, page, pageSize);
            return Ok(partyCategoryMenus);
        }

        [HttpPost]
        public async Task<ActionResult> CreatePartyCategoryMenu([FromBody] PartyCategoryMenuCreateRequest request)
        {
            var result = await _partyCategoryMenuService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePartyCategoryMenu(int id, [FromBody] PartyCategoryMenuUpdateRequest request)
        {
            var result = await _partyCategoryMenuService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePartyCategoryMenu(int id)
        {
            var result = await _partyCategoryMenuService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
