using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookfetSystem.Repositories;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.ZaloPay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookfetSystem.Services.Implement
{
    public class ZaloPayWebhookService : IZaloPayWebhookService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZaloPayWebhookService> _logger;

        public ZaloPayWebhookService(
            PaymentRepository paymentRepository,
            OrderRepository orderRepository,
            IConfiguration configuration,
            ILogger<ZaloPayWebhookService> logger)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(int ReturnCode, string ReturnMessage)> ProcessAsync(ZaloPayCallbackPayload payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.Data) || string.IsNullOrWhiteSpace(payload.Mac))
            {
                return (-1, "invalid payload");
            }

            var key2 = _configuration["ZaloPay:Key2"];
            if (string.IsNullOrWhiteSpace(key2))
            {
                _logger.LogError("Missing ZaloPay:Key2 for callback verification.");
                return (-1, "missing key2");
            }

            var expectedMac = ComputeHmacSha256(key2, payload.Data);
            if (!string.Equals(expectedMac, payload.Mac, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid ZaloPay callback MAC.");
                return (-1, "mac not equal");
            }

            ZaloPayPaidData? callbackData;
            try
            {
                callbackData = JsonSerializer.Deserialize<ZaloPayPaidData>(payload.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid ZaloPay callback data format.");
                return (0, "invalid callback data");
            }

            if (callbackData == null || string.IsNullOrWhiteSpace(callbackData.AppTransId))
            {
                return (0, "missing app_trans_id");
            }

            if (!TryExtractPaymentId(callbackData.AppTransId!, out var paymentId))
            {
                _logger.LogWarning("Cannot extract paymentId from app_trans_id={AppTransId}", callbackData.AppTransId);
                return (0, "invalid app_trans_id");
            }

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found by paymentId={PaymentId}", paymentId);
                return (2, "payment not found");
            }

            if (string.Equals(payment.PaymentStatus, PaymentStatus.PAID.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (2, "payment already processed");
            }

            payment.PaymentStatus = PaymentStatus.PAID.ToString();
            payment.PaidAt = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            if (payment.OrderId.HasValue)
            {
                var order = await _orderRepository.GetByIdAsync(payment.OrderId.Value);
                if (order != null)
                {
                    var zpTransId = GetZpTransIdAsString(callbackData.ZpTransId);
                    if (!string.IsNullOrWhiteSpace(zpTransId))
                    {
                        var metadata = DeserializeZaloPayMetadata(order.MtdZlp);
                        var paymentMeta = metadata.Payments.FirstOrDefault(x => x.PaymentId == payment.PaymentId);
                        if (paymentMeta == null)
                        {
                            paymentMeta = new ZaloPayPaymentMetadata
                            {
                                PaymentId = payment.PaymentId
                            };
                            metadata.Payments.Add(paymentMeta);
                        }

                        paymentMeta.AppTransId = callbackData.AppTransId;
                        paymentMeta.ZpTransId = zpTransId;
                        order.MtdZlp = JsonSerializer.Serialize(metadata);
                    }

                    if (string.Equals(payment.PaymentType, PaymentType.DEPOSIT.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        var depositAmount = payment.Amount ?? 0m;
                        var totalPrice = order.TotalPrice ?? 0m;
                        order.DepositAmount = depositAmount;
                        order.RemainingAmount = totalPrice - depositAmount;
                    }
                    else if (string.Equals(payment.PaymentType, PaymentType.FULL.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        order.RemainingAmount = 0;
                        order.Status = OrderStatus.COMPLETED.ToString();
                    }

                    await _orderRepository.UpdateAsync(order);
                }
            }

            return (1, "success");
        }

        private static string ComputeHmacSha256(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static bool TryExtractPaymentId(string appTransId, out int paymentId)
        {
            paymentId = 0;
            var parts = appTransId.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                return false;
            }

            return int.TryParse(parts[2], out paymentId) && paymentId > 0;
        }

        private sealed class ZaloPayPaidData
        {
            [JsonPropertyName("app_trans_id")]
            public string? AppTransId { get; set; }

            [JsonPropertyName("zp_trans_id")]
            public JsonElement? ZpTransId { get; set; }
        }

        private static ZaloPayMetadata DeserializeZaloPayMetadata(string? rawMetadata)
        {
            if (string.IsNullOrWhiteSpace(rawMetadata))
            {
                return new ZaloPayMetadata();
            }

            try
            {
                return JsonSerializer.Deserialize<ZaloPayMetadata>(rawMetadata) ?? new ZaloPayMetadata();
            }
            catch
            {
                return new ZaloPayMetadata();
            }
        }

        private static string? GetZpTransIdAsString(JsonElement? zpTransIdElement)
        {
            if (!zpTransIdElement.HasValue)
            {
                return null;
            }

            var element = zpTransIdElement.Value;
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.ToString(),
                _ => null
            };
        }

        private sealed class ZaloPayMetadata
        {
            public List<ZaloPayPaymentMetadata> Payments { get; set; } = new();
        }

        private sealed class ZaloPayPaymentMetadata
        {
            public int PaymentId { get; set; }
            public string? AppTransId { get; set; }
            public string? ZpTransId { get; set; }
        }
    }
}
