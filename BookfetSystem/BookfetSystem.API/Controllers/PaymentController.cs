using System;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.SePay;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ISePayWebhookService _sePayWebhookService;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IPaymentService paymentService,
            ISePayWebhookService sePayWebhookService,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _sePayWebhookService = sePayWebhookService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPaymentsFiltered([FromQuery] PaymentFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var payments = await _paymentService.GetAllPaymentFilteredAsync(filter, page, pageSize);
            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult> CreatePayment([FromBody] PaymentCreateRequest request)
        {
            var result = await _paymentService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePayment(int id, [FromBody] PaymentUpdateRequest request)
        {
            var result = await _paymentService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var result = await _paymentService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }

        [HttpPost("create-deposit-qr/{orderId}")]
        public async Task<ActionResult> CreateDepositQR(int orderId)
        {
            var result = await _paymentService.CreateDepositQR(orderId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("sepay-webhook")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookPayload payload)
        {
            if (payload == null)
                return BadRequest(new { success = false });

            var webhookKey = _configuration["SePay:WebhookApiKey"];
            if (!string.IsNullOrEmpty(webhookKey))
            {
                if (!Request.Headers.TryGetValue("Authorization", out var auth) ||
                    !auth.ToString().Contains($"Apikey {webhookKey}", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { success = false });
                }
            }

            await _sePayWebhookService.ProcessAsync(payload);
            return Ok(new { success = true });
        }
    }
}
