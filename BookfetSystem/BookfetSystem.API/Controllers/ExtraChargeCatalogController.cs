using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/extra-charge-catalog")]
    [ApiController]
    public class ExtraChargeCatalogController : ControllerBase
    {
        private readonly IExtraChargeCatalogService _extraChargeCatalogService;

        public ExtraChargeCatalogController(IExtraChargeCatalogService extraChargeCatalogService)
        {
            _extraChargeCatalogService = extraChargeCatalogService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFiltered([FromQuery] ExtraChargeCatalogFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _extraChargeCatalogService.GetAllFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ExtraChargeCatalogCreateRequest request)
        {
            var result = await _extraChargeCatalogService.CreateAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ExtraChargeCatalogUpdateRequest request)
        {
            var result = await _extraChargeCatalogService.UpdateAsync(id, request);
            if (result.Success)
                return Ok(result);
            if (result.Message == "Extra charge catalog not found.")
                return NotFound(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _extraChargeCatalogService.DeleteAsync(id);
            if (result.Success)
                return NoContent();
            if (result.Message == "Extra charge catalog not found.")
                return NotFound(result);
            return BadRequest(result);
        }
    }
}
