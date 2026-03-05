using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllServices(
            [FromQuery] ServiceFilterRequest filter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _serviceService.GetAllServiceFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateService([FromBody] ServiceCreateRequest request)
        {
            var result = await _serviceService.CreateAsync(request);

            if ((bool)result.GetType().GetProperty("Success").GetValue(result))
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateService(int id, [FromBody] ServiceUpdateRequest request)
        {
            var result = await _serviceService.UpdateAsync(id, request);

            if ((bool)result.GetType().GetProperty("Success").GetValue(result))
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteService(int id)
        {
            var result = await _serviceService.DeleteAsync(id);

            if ((bool)result.GetType().GetProperty("Success").GetValue(result))
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}