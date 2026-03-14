using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
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
        public async Task<ActionResult> GetAllOrders([FromQuery] OrderFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.GetAllFilteredAsync(filter, page, pageSize);
            return Ok(orders);
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult> GetOrderById(int id)
        //{
        //    var order = await _orderService.GetById(id);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(order);
        //}

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {
            var result = await _orderService.Create(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(int id, [FromBody] OrderUpdateRequest request)
        {
            var result = await _orderService.Update(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var result = await _orderService.Delete(id);

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Order not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateOrderAsync([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("owner/assignable")]
        public async Task<ActionResult> GetDepositedApprovedOrdersForAssignment([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.GetDepositedApprovedForAssignmentAsync(page, pageSize);
            return Ok(orders);
        }

        [HttpPut("{orderId}/assign-staff-group")]
        public async Task<ActionResult> AssignOrderToStaffGroup(int orderId, [FromBody] AssignOrderStaffGroupRequest request)
        {
            var result = await _orderService.AssignOrderToStaffGroupAsync(orderId, request.StaffGroupId);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Order not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
    }
}