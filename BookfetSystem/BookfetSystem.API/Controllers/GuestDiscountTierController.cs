using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/guest-discount-tier")]
    [ApiController]
    public class GuestDiscountTierController : ControllerBase
    {
        private readonly IGuestDiscountTierService _guestDiscountTierService;

        public GuestDiscountTierController(IGuestDiscountTierService guestDiscountTierService)
        {
            _guestDiscountTierService = guestDiscountTierService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFiltered([FromQuery] GuestDiscountTierFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _guestDiscountTierService.GetAllFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] GuestDiscountTierCreateRequest request)
        {
            var result = await _guestDiscountTierService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] GuestDiscountTierUpdateRequest request)
        {
            var result = await _guestDiscountTierService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Guest discount tier not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _guestDiscountTierService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            if (result.Message == "Guest discount tier not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
    }
}
