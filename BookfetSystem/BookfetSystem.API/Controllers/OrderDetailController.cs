using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order-detail")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllOrderDetails([FromQuery] OrderDetailRequest filter)
        {
            var orderDetails = await _orderDetailService.GetAll(filter);
            return Ok(orderDetails);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrderDetailById(int id)
        {
            var orderDetail = await _orderDetailService.GetById(id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            return Ok(orderDetail);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrderDetail([FromBody] OrderDetailRequest request)
        {
            var result = await _orderDetailService.Create(request);

            if (result)
            {
                return Ok("Order detail created successfully");
            }

            return BadRequest("Create order detail failed");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetail(int id, [FromBody] OrderDetailRequest request)
        {
            if (id != request.OrderDetailId)
            {
                return BadRequest("Id mismatch");
            }

            var result = await _orderDetailService.Update(request);

            if (result)
            {
                return Ok("Order detail updated successfully");
            }

            return BadRequest("Update order detail failed");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetail(int id)
        {
            var result = await _orderDetailService.Delete(id);

            if (result)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}