using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order-detail-custom")]
    [ApiController]
    public class OrderDetailCustomController : ControllerBase
    {
        private readonly IOrderDetailCustomService _orderDetailCustomService;

        public OrderDetailCustomController(IOrderDetailCustomService orderDetailCustomService)
        {
            _orderDetailCustomService = orderDetailCustomService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllOrderDetailCustomsFiltered([FromQuery] OrderDetailCustomFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderDetailCustomService.GetAllOrderDetailCustomFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrderDetailCustom([FromBody] OrderDetailCustomCreateRequest request)
        {
            var result = await _orderDetailCustomService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetailCustom(int id, [FromBody] OrderDetailCustomUpdateRequest request)
        {
            var result = await _orderDetailCustomService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetailCustom(int id)
        {
            var result = await _orderDetailCustomService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
