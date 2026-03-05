using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] OrderFilterRequest filter,
                                                [FromQuery] int page = 1,
                                                [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetAllFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateRequest request)
        {
            var result = await _orderService.CreateAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, OrderUpdateRequest request)
        {
            var result = await _orderService.UpdateAsync(id, request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _orderService.DeleteAsync(id);

            if (result.Success)
                return NoContent();

            return NotFound(result);
        }
    }
}