using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order-detail-extra-charge")]
    [ApiController]
    public class OrderDetailExtraChargeController : ControllerBase
    {
        private readonly IOrderDetailExtraChargeService _orderDetailExtraChargeService;

        public OrderDetailExtraChargeController(IOrderDetailExtraChargeService orderDetailExtraChargeService)
        {
            _orderDetailExtraChargeService = orderDetailExtraChargeService;
        }

        [HttpGet("catalog/active")]
        public async Task<ActionResult> GetActiveCatalog([FromQuery] int? serviceId)
        {
            var result = await _orderDetailExtraChargeService.GetActiveCatalogAsync(serviceId);
            return Ok(result);
        }

        [HttpGet("catalog/active/by-order-detail/{orderDetailId}")]
        public async Task<ActionResult> GetActiveCatalogByOrderDetail(int orderDetailId)
        {
            var result = await _orderDetailExtraChargeService.GetActiveCatalogByOrderDetailAsync(orderDetailId);
            return Ok(result);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult> GetByOrderId(int orderId)
        {
            var result = await _orderDetailExtraChargeService.GetByOrderIdAsync(orderId);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> Create([FromForm] OrderDetailExtraChargeCreateRequest request)
        {
            var leaderIdValue = User.FindFirst("Id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(leaderIdValue, out var leaderId))
            {
                return Unauthorized(new { Message = "Invalid token: missing leader id." });
            }

            var result = await _orderDetailExtraChargeService.CreateAsync(request, leaderId);
            if (result.Success)
            {
                return Ok(result);
            }

            if (result.Message == "Order detail not found." || result.Message == "Extra charge catalog not found.")
            {
                return NotFound(result);
            }

            return BadRequest(result);
        }
    }
}
