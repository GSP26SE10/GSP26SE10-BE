using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/service-extra-charge-catalog")]
    [ApiController]
    [Authorize]
    public class ServiceExtraChargeCatalogController : ControllerBase
    {
        private readonly IServiceExtraChargeCatalogService _serviceExtraChargeCatalogService;

        public ServiceExtraChargeCatalogController(IServiceExtraChargeCatalogService serviceExtraChargeCatalogService)
        {
            _serviceExtraChargeCatalogService = serviceExtraChargeCatalogService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFiltered([FromQuery] ServiceExtraChargeCatalogFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var authError = EnsureAdmin();
            if (authError != null)
            {
                return authError;
            }

            var result = await _serviceExtraChargeCatalogService.GetAllFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ServiceExtraChargeCatalogCreateRequest request)
        {
            var authError = EnsureAdmin();
            if (authError != null)
            {
                return authError;
            }

            var result = await _serviceExtraChargeCatalogService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ServiceExtraChargeCatalogUpdateRequest request)
        {
            var authError = EnsureAdmin();
            if (authError != null)
            {
                return authError;
            }

            var result = await _serviceExtraChargeCatalogService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Service-extra charge catalog mapping not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var authError = EnsureAdmin();
            if (authError != null)
            {
                return authError;
            }

            var result = await _serviceExtraChargeCatalogService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            if (result.Message == "Service-extra charge catalog mapping not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        private ActionResult? EnsureAdmin()
        {
            var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!int.TryParse(roleValue, out var roleId))
            {
                return Unauthorized(new { Message = "Invalid token: missing role id." });
            }

            if (roleId != 1)
            {
                return StatusCode(403, new { Message = "Only admin role can manage service-extra charge catalog mappings." });
            }

            return null;
        }
    }
}
