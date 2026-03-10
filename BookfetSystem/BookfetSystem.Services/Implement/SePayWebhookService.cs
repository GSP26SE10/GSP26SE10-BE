using System.Text.RegularExpressions;
using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.SePay;

namespace BookfetSystem.Services.Implement
{
    public class SePayWebhookService : ISePayWebhookService
    {
        private readonly PaymentRepository _paymentRepository;

        public SePayWebhookService(PaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<bool> ProcessAsync(SePayWebhookPayload payload)
        {
            if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
                return true;

            var orderId = ExtractOrderId(payload.Code) ?? ExtractOrderId(payload.Content);
            if (!orderId.HasValue)
                return true;

            var payment = await _paymentRepository.GetUnpaidDepositByOrderIdAsync(orderId.Value);
            if (payment == null)
                return true;

            if (payload.TransferAmount < (payment.Amount ?? 0))
                return true;

            payment.PaymentStatus = PaymentStatus.PAID.ToString();
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

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
