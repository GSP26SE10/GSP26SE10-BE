using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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


        [HttpPut("{orderId}/customer-edit")]
        public async Task<ActionResult> UpdateCustomerOrder(int orderId, [FromBody] UpdateCustomerOrderRequest request)
        {
            var result = await _orderService.UpdateCustomerOrderAsync(orderId, request);
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

        [Authorize]
        [HttpPut("{orderId}/owner/review")]
        public async Task<ActionResult> ReviewOrder(int orderId, [FromBody] ReviewOrderRequest request)
        {
            var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!int.TryParse(roleValue, out var roleId))
            {
                return Unauthorized(new { Message = "Invalid token: missing role id." });
            }

            if (roleId != 1)
            {
                return StatusCode(403, new { Message = "Only admin role can review order." });
            }

            var reviewerIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(reviewerIdValue, out var reviewerId))
            {
                return Unauthorized(new { Message = "Invalid token: missing reviewer id." });
            }

            var result = await _orderService.ReviewOrderAsync(orderId, request.Status, reviewerId);
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