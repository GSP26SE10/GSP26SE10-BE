using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _service;

        public ServiceController(IServiceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ServiceFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var result = await _service.GetById(id);

        //    if (result == null)
        //        return NotFound();

        //    return Ok(result);
        //}

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ServiceCreateRequest request)
        {
            var result = await _service.Create(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] ServiceUpdateRequest request)
        {
            var result = await _service.Update(id, request);

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Service not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Service not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
    }
}