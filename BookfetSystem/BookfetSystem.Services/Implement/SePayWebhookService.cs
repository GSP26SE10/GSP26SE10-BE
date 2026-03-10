using System.Text.RegularExpressions;
using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.SePay;
using Microsoft.Extensions.Logging;

namespace BookfetSystem.Services.Implement
{
    public class SePayWebhookService : ISePayWebhookService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly ILogger<SePayWebhookService> _logger;

        public SePayWebhookService(PaymentRepository paymentRepository, ILogger<SePayWebhookService> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<bool> ProcessAsync(SePayWebhookPayload payload)
        {
            _logger.LogInformation("SePay webhook received: id={Id}, transferType={Type}, code={Code}, content={Content}, amount={Amount}",
                payload.Id, payload.TransferType, payload.Code, payload.Content, payload.TransferAmount);

            if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Ignored: transferType is not 'in'");
                return true;
            }

            var orderId = ExtractOrderId(payload.Code) ?? ExtractOrderId(payload.Content);
            if (!orderId.HasValue)
            {
                _logger.LogWarning("No orderId found in code or content");
                return true;
            }

            var payment = await _paymentRepository.GetUnpaidDepositByOrderIdAsync(orderId.Value);
            if (payment == null)
            {
                _logger.LogWarning("No unpaid deposit found for orderId={OrderId}", orderId.Value);
                return true;
            }

            if (payload.TransferAmount < (payment.Amount ?? 0))
            {
                _logger.LogWarning("Transfer amount {Amount} less than required {Required} for orderId={OrderId}",
                    payload.TransferAmount, payment.Amount, orderId.Value);
                return true;
            }

            payment.PaymentStatus = PaymentStatus.PAID.ToString();
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);
            _logger.LogInformation("Payment {PaymentId} marked PAID for orderId={OrderId}", payment.PaymentId, orderId.Value);

            return true;
        }

        private static int? ExtractOrderId(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var match = Regex.Match(text, @"BOOKFET_(\d+)", RegexOptions.IgnoreCase);
            return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }
    }
}
