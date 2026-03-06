using BookfetSystem.Services.Interfaces;
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
        public async Task<ActionResult> GetAllOrderDetailCustoms([FromQuery] OrderDetailCustomFilterRequest filter)
        {
            var result = await _orderDetailCustomService.GetAll(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderDetailCustomById(int id)
        {
            var result = await _orderDetailCustomService.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrderDetailCustom([FromBody] OrderDetailCustomCreateRequest request)
        {
            var result = await _orderDetailCustomService.Create(request);

            if (result)
            {
                return Ok("OrderDetailCustom created successfully");
            }

            return BadRequest("Create failed");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetailCustom(int id, [FromBody] OrderDetailCustomUpdateRequest request)
        {
            if (id != request.OrderDetailCustomId)
            {
                return BadRequest("Id mismatch");
            }

            var result = await _orderDetailCustomService.Update(request);

            if (result)
            {
                return Ok("OrderDetailCustom updated successfully");
            }

            return BadRequest("Update failed");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetailCustom(int id)
        {
            var result = await _orderDetailCustomService.Delete(id);

            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}