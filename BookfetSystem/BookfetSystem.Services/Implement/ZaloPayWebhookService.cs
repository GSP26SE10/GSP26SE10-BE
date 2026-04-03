using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.ZaloPay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookfetSystem.Services.Implement
{
    public class ZaloPayWebhookService : IZaloPayWebhookService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<ZaloPayWebhookService> _logger;

        public ZaloPayWebhookService(
            PaymentRepository paymentRepository,
            OrderRepository orderRepository,
            UserRepository userRepository,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<ZaloPayWebhookService> logger)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
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

                    if (string.Equals(payment.PaymentType, PaymentType.DEPOSIT.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        await SendOrderDepositSuccessEmailsAsync(order.OrderId, "ZaloPay");
                    }
                }
            }

            return (1, "success");
        }

        private async Task SendOrderDepositSuccessEmailsAsync(int orderId, string paymentMethodLabel)
        {
            var orderWithCustomer = await _orderRepository.GetByIdWithRelationAsync(orderId);
            if (orderWithCustomer == null)
            {
                return;
            }

            var customerName = string.IsNullOrWhiteSpace(orderWithCustomer.Customer?.FullName)
                ? "Quý khách"
                : orderWithCustomer.Customer!.FullName;
            var (partyCardsHtml, partyPlainText) = BuildPartyCards(orderWithCustomer);
            var depositAmountText = $"{(orderWithCustomer.DepositAmount ?? 0m):N0} VND";
            var remainingAmountText = $"{(orderWithCustomer.RemainingAmount ?? 0m):N0} VND";

            var customerEmail = orderWithCustomer.Customer?.Email;
            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                var customerSubject = $"[Bookfet] Đơn hàng #{orderWithCustomer.OrderId} đã đặt cọc thành công";
                var customerHtmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:16px;background:#f8fafc;color:#0f172a;"">
  <div style=""background:#ffffff;border-radius:12px;padding:20px;border:1px solid #e2e8f0;"">
    <h2 style=""margin:0 0 12px 0;"">Đặt cọc thành công</h2>
    <p style=""margin:0 0 10px 0;"">Xin chào <strong>{customerName}</strong>,</p>
    <p style=""margin:0 0 12px 0;"">Bookfet đã ghi nhận thanh toán tiền cọc cho đơn hàng <strong>#{orderWithCustomer.OrderId}</strong> qua <strong>{paymentMethodLabel}</strong>.</p>
    <p style=""margin:0 0 14px 0;"">
      Trạng thái:
      <span style=""display:inline-block;padding:4px 10px;border-radius:999px;background:#fef9c3;color:#854d0e;font-weight:700;"">
        CHỜ DUYỆT
      </span>
    </p>
    <div style=""margin:12px 0 14px 0;padding:12px;border-radius:10px;background:#eff6ff;border:1px solid #bfdbfe;color:#1e3a8a;"">
      <p style=""margin:0 0 8px 0;font-weight:700;"">Thông tin thanh toán</p>
      <p style=""margin:0;"">Tiền cọc đã thanh toán: <strong>{depositAmountText}</strong></p>
      <p style=""margin:6px 0 0 0;"">Số tiền còn lại: <strong>{remainingAmountText}</strong></p>
    </div>
    <div style=""margin:14px 0;"">
      <p style=""margin:0 0 8px 0;font-weight:700;"">Các tiệc trong đơn hàng của bạn:</p>
      {partyCardsHtml}
    </div>
    <p style=""margin:0;"">Đội ngũ Bookfet sẽ sớm kiểm tra và duyệt đơn. Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi.</p>
  </div>
</div>";
                var customerPlainText =
                    $"Xin chao {customerName}. Don hang #{orderWithCustomer.OrderId} da dat coc thanh cong qua {paymentMethodLabel}. Tien coc: {depositAmountText}. So tien con lai: {remainingAmountText}. Cac tiec: {partyPlainText}. Trang thai hien tai: CHO DUYET.";
                try
                {
                    await _emailService.SendAsync(customerEmail, customerSubject, customerHtmlBody, customerPlainText);
                }
                catch
                {
                    _logger.LogWarning("Failed to send deposit-success email to customer for orderId={OrderId}", orderWithCustomer.OrderId);
                }
            }

            var adminEmails = await _userRepository
                .GetAllUserFiltered(new User { RoleId = 1 })
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .Select(x => x.Email!)
                .Distinct()
                .ToListAsync();

            if (adminEmails.Count == 0)
            {
                return;
            }

            var adminSubject = $"[Bookfet][Admin] Có đơn hàng mới chờ duyệt #{orderWithCustomer.OrderId}";
            var adminHtmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:16px;background:#f8fafc;color:#0f172a;"">
  <div style=""background:#ffffff;border-radius:12px;padding:20px;border:1px solid #e2e8f0;"">
    <h2 style=""margin:0 0 12px 0;"">Đơn hàng mới cần duyệt</h2>
    <p style=""margin:0 0 10px 0;"">Hệ thống vừa ghi nhận đơn hàng <strong>#{orderWithCustomer.OrderId}</strong> đã thanh toán cọc thành công.</p>
    <p style=""margin:0 0 8px 0;"">Khách hàng: <strong>{customerName}</strong></p>
    <p style=""margin:0 0 14px 0;"">
      Trạng thái:
      <span style=""display:inline-block;padding:4px 10px;border-radius:999px;background:#fef9c3;color:#854d0e;font-weight:700;"">
        CHỜ ADMIN DUYỆT
      </span>
    </p>
    <div style=""margin:12px 0 14px 0;padding:12px;border-radius:10px;background:#ecfdf5;border:1px solid #bbf7d0;color:#166534;"">
      <p style=""margin:0 0 8px 0;font-weight:700;"">Tổng quan thanh toán</p>
      <p style=""margin:0;"">Tiền cọc đã thu: <strong>{depositAmountText}</strong></p>
      <p style=""margin:6px 0 0 0;"">Số tiền còn lại: <strong>{remainingAmountText}</strong></p>
    </div>
    <div style=""margin:14px 0;"">
      <p style=""margin:0 0 8px 0;font-weight:700;"">Danh sách tiệc cần duyệt:</p>
      {partyCardsHtml}
    </div>
    <p style=""margin:0;"">Vui lòng vào hệ thống để duyệt đơn sớm cho khách hàng.</p>
  </div>
</div>";
            var adminPlainText =
                $"Don hang moi #{orderWithCustomer.OrderId} da dat coc thanh cong va dang cho duyet. Khach hang: {customerName}. Tien coc: {depositAmountText}. So tien con lai: {remainingAmountText}. Cac tiec: {partyPlainText}.";

            foreach (var adminEmail in adminEmails)
            {
                try
                {
                    await _emailService.SendAsync(adminEmail, adminSubject, adminHtmlBody, adminPlainText);
                }
                catch
                {
                    _logger.LogWarning("Failed to send new-order admin email for orderId={OrderId}, admin={AdminEmail}", orderWithCustomer.OrderId, adminEmail);
                }
            }
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

        private static (string Html, string PlainText) BuildPartyCards(Order order)
        {
            var cards = (order.OrderDetails ?? new List<OrderDetail>())
                .OrderBy(x => x.StartTime)
                .Select(detail =>
                {
                    var startTimeText = detail.StartTime.HasValue
                        ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                        : "Chưa xác định";
                    var menuName = string.IsNullOrWhiteSpace(detail.Menu?.MenuName) ? "Tiệc đơn giản" : detail.Menu!.MenuName;
                    var menuImageUrl = GetFirstImageUrl(detail.Menu?.ImgUrl);
                    var imageHtml = string.IsNullOrWhiteSpace(menuImageUrl)
                        ? @"<div style=""height:120px;border-radius:10px;background:#e2e8f0;color:#334155;display:flex;align-items:center;justify-content:center;font-weight:600;"">Tiệc đơn giản</div>"
                        : $@"<img src=""{menuImageUrl}"" alt=""{menuName}"" style=""width:100%;height:120px;object-fit:cover;border-radius:10px;display:block;"" />";

                    var html = $@"
<div style=""border:1px solid #e2e8f0;border-radius:10px;padding:12px;margin-bottom:10px;background:#f8fafc;"">
  {imageHtml}
  <p style=""margin:10px 0 4px 0;font-weight:700;"">{menuName}</p>
  <p style=""margin:0;color:#334155;"">Mã tiệc: <strong>#{detail.OrderDetailId}</strong></p>
  <p style=""margin:4px 0 0 0;color:#334155;"">Thời gian bắt đầu: <strong>{startTimeText}</strong></p>
</div>";
                    var plainText = $"tiec #{detail.OrderDetailId} ({menuName}) bat dau luc {startTimeText}";
                    return (html, plainText);
                })
                .ToList();

            return (
                cards.Count == 0 ? @"<p style=""margin:0;color:#334155;"">Chưa có thông tin tiệc.</p>" : string.Join(string.Empty, cards.Select(x => x.html)),
                cards.Count == 0 ? "chua co thong tin tiec" : string.Join("; ", cards.Select(x => x.plainText))
            );
        }

        private static DateTime ToVietnamTime(DateTime input)
        {
            var utc = input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utc, GetVietnamTimeZone());
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        private static string? GetFirstImageUrl(string? rawImgUrl)
        {
            if (string.IsNullOrWhiteSpace(rawImgUrl))
            {
                return null;
            }

            var trimmed = rawImgUrl.Trim();
            try
            {
                if (trimmed.StartsWith("["))
                {
                    var images = JsonSerializer.Deserialize<List<string>>(trimmed);
                    return images?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                }

                if (trimmed.StartsWith("\""))
                {
                    var single = JsonSerializer.Deserialize<string>(trimmed);
                    return string.IsNullOrWhiteSpace(single) ? null : single;
                }
            }
            catch
            {
                // fallback below when image JSON is malformed.
            }

            return trimmed;
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
