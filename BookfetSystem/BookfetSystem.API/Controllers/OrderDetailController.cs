using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        public async Task<ActionResult> GetAllOrderDetails([FromQuery] OrderDetailFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var orderDetails = await _orderDetailService.GetAllFilteredAsync(filter, page, pageSize);
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
        public async Task<ActionResult> CreateOrderDetail([FromBody] OrderDetailCreateRequest request)
        {
            var result = await _orderDetailService.Create(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetail(int id, [FromBody] OrderDetailUpdateRequest request)
        {
            var result = await _orderDetailService.Update(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Order detail not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetail(int id)
        {
            var result = await _orderDetailService.Delete(id);

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Order detail not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpPut("end-early/{id}")]
        public async Task<ActionResult> EndOrderDetailEarly(int id)
        {
            var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!int.TryParse(roleValue, out var roleId))
            {
                return Unauthorized(new { Message = "Invalid token: missing role id." });
            }

            if (roleId != 2)
            {
                return StatusCode(403, new { Message = "Only leader role can end order detail early." });
            }

            var result = await _orderDetailService.EndEarlyByLeaderAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Order detail not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
    }
}