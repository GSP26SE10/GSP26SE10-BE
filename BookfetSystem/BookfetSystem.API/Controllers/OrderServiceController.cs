using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order-service")]
    [ApiController]
    public class OrderServiceController : ControllerBase
    {
        private readonly IOrderServiceManager _orderServiceManager;

        public OrderServiceController(IOrderServiceManager orderServiceManager)
        {
            _orderServiceManager = orderServiceManager;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllOrderServicesFiltered([FromQuery] OrderServiceFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderServiceManager.GetAllOrderServiceFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrderService([FromBody] OrderServiceCreateRequest request)
        {
            var result = await _orderServiceManager.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderService(int id, [FromBody] OrderServiceUpdateRequest request)
        {
            var result = await _orderServiceManager.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderService(int id)
        {
            var result = await _orderServiceManager.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
