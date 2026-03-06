using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _service;

        public ServiceController(IServiceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ServiceFilterRequest filter)
        {
            var result = await _service.GetAll(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ServiceCreateRequest request)
        {
            var result = await _service.Create(request);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(ServiceUpdateRequest request)
        {
            var result = await _service.Update(request);

            if (!result)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result)
                return NotFound();

            return Ok(result);
        }
    }
}