using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using static BookfetSystem.Services.Models.Request.PaymentRequest;

namespace BookfetSystem.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFiltered(
            [FromQuery] PaymentFilterRequest filter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _paymentService.GetAllFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PaymentCreateRequest request)
        {
            var result = await _paymentService.CreateAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] PaymentUpdateRequest request)
        {
            var result = await _paymentService.UpdateAsync(id, request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _paymentService.DeleteAsync(id);

            if (result.Success)
                return NoContent();

            return NotFound(result);
        }
    }
}