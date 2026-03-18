using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/contact-request")]
    [ApiController]
    public class ContactRequestController : ControllerBase
    {
        private readonly IContactRequestService _service;

        public ContactRequestController(IContactRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ContactRequestFilterRequest filter, int page = 1, int pageSize = 10)
        {
            return Ok(await _service.GetAllFilteredAsync(filter, page, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContactRequestCreateRequest request)
        {
            var result = await _service.CreateAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ContactRequestUpdateRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result.Success ? NoContent() : NotFound(result);
        }
    }
}