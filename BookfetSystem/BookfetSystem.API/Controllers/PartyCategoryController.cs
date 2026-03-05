using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/party-category")]
    [ApiController]
    public class PartyCategoryController : ControllerBase
    {
        private readonly IPartyCategoryService _partyCategoryService;

        public PartyCategoryController(IPartyCategoryService partyCategoryService)
        {
            _partyCategoryService = partyCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPartyCategoriesFiltered(
            [FromQuery] PartyCategoryFilterRequest filter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _partyCategoryService
                .GetAllPartyCategoryFilteredAsync(filter, page, pageSize);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreatePartyCategory([FromBody] PartyCategoryCreateRequest request)
        {
            var result = await _partyCategoryService.CreateAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePartyCategory(int id, [FromBody] PartyCategoryUpdateRequest request)
        {
            var result = await _partyCategoryService.UpdateAsync(id, request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePartyCategory(int id)
        {
            var result = await _partyCategoryService.DeleteAsync(id);

            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}