using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Request.BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ICustomerOrderService _orderService;

        public OrderController(ICustomerOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllOrders([FromQuery] OrderFilterRequest filter)
        {
            var orders = await _orderService.GetAll(filter);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetById(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var result = await _orderService.Create(request);

            if (result)
            {
                return Ok("Order created successfully");
            }

            return BadRequest("Create order failed");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(int id, [FromBody] OrderUpdateRequest request)
        {
            if (id != request.OrderId)
            {
                return BadRequest("Id mismatch");
            }

            var result = await _orderService.Update(request);

            if (result)
            {
                return Ok("Order updated successfully");
            }

            return BadRequest("Update order failed");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var result = await _orderService.Delete(id);

            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}