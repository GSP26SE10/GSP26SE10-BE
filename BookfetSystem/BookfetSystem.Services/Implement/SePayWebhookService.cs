using System.Text.RegularExpressions;
using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.SePay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BookfetSystem.Services.Implement
{
    public class SePayWebhookService : ISePayWebhookService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;
        private readonly UserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<SePayWebhookService> _logger;

        public SePayWebhookService(PaymentRepository paymentRepository, OrderRepository orderRepository, UserRepository userRepository, IEmailService emailService, ILogger<SePayWebhookService> logger)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _emailService = emailService;
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

            var paymentType = ExtractPaymentType(payload.Code) ?? ExtractPaymentType(payload.Content) ?? PaymentType.DEPOSIT;
            var payment = await _paymentRepository.GetUnpaidByOrderIdAndTypeAsync(orderId.Value, paymentType.ToString());
            if (payment == null)
            {
                _logger.LogWarning("No unpaid {PaymentType} payment found for orderId={OrderId}", paymentType, orderId.Value);
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

            var order = await _orderRepository.GetByIdAsync(orderId.Value);
            if (order != null)
            {
                if (paymentType == PaymentType.DEPOSIT)
                {
                    var depositAmount = payment.Amount ?? 0;
                    var totalPrice = order.TotalPrice ?? 0;
                    order.DepositAmount = depositAmount;
                    order.RemainingAmount = totalPrice - depositAmount;
                    await _orderRepository.UpdateAsync(order);
                    await SendOrderDepositSuccessEmailsAsync(order.OrderId, "chuyển khoản QR");
                    _logger.LogInformation("Order {OrderId} updated after DEPOSIT: DepositAmount={Deposit}, RemainingAmount={Remaining}",
                        orderId.Value, depositAmount, order.RemainingAmount);
                }
                else if (paymentType == PaymentType.FULL)
                {
                    order.RemainingAmount = 0;
                    order.Status = OrderStatus.COMPLETED.ToString();
                    await _orderRepository.UpdateAsync(order);
                    await SendOrderCompletedEmailAsync(order.OrderId, "quét mã QR");
                    _logger.LogInformation("Order {OrderId} updated after FULL payment: RemainingAmount={Remaining}, Status={Status}",
                        orderId.Value, order.RemainingAmount, order.Status);
                }
            }

            return true;
        }

        private static int? ExtractOrderId(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var normalized = NormalizePaymentText(text);
            var match = Regex.Match(normalized, @"BOOKFET(?:FULL|DEPOSIT)?(\d+)", RegexOptions.IgnoreCase);
            return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        private static PaymentType? ExtractPaymentType(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var normalized = NormalizePaymentText(text);

            if (Regex.IsMatch(normalized, @"BOOKFETFULL\d+", RegexOptions.IgnoreCase))
            {
                return PaymentType.FULL;
            }

            if (Regex.IsMatch(normalized, @"BOOKFET(?:DEPOSIT)?\d+", RegexOptions.IgnoreCase))
            {
                return PaymentType.DEPOSIT;
            }

            return null;
        }

        private static string NormalizePaymentText(string text)
        {
            return Regex.Replace(text, "[^a-zA-Z0-9]", string.Empty).ToUpperInvariant();
        }

        private async Task SendOrderCompletedEmailAsync(int orderId, string paymentMethodLabel)
        {
            var orderWithCustomer = await _orderRepository.GetByIdWithRelationAsync(orderId);
            var toEmail = orderWithCustomer?.Customer?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var customerName = string.IsNullOrWhiteSpace(orderWithCustomer.Customer?.FullName)
                ? "Quý khách"
                : orderWithCustomer.Customer.FullName;
            var partyCards = (orderWithCustomer.OrderDetails ?? new List<OrderDetail>())
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

                    return $@"
<div style=""border:1px solid #e2e8f0;border-radius:10px;padding:12px;margin-bottom:10px;background:#f8fafc;"">
  {imageHtml}
  <p style=""margin:10px 0 4px 0;font-weight:700;"">{menuName}</p>
  <p style=""margin:0;color:#334155;"">Mã tiệc: <strong>#{detail.OrderDetailId}</strong></p>
  <p style=""margin:4px 0 0 0;color:#334155;"">Thời gian tổ chức: <strong>{startTimeText}</strong></p>
</div>";
                })
                .ToList();
            var partySection = partyCards.Count == 0
                ? string.Empty
                : $@"
<div style=""margin:14px 0;"">
  <p style=""margin:0 0 8px 0;font-weight:700;"">Các tiệc đã được tổ chức và hoàn thành:</p>
  {string.Join(string.Empty, partyCards)}
</div>";
            var partyPlainText = string.Join(
                "; ",
                (orderWithCustomer.OrderDetails ?? new List<OrderDetail>())
                    .OrderBy(x => x.StartTime)
                    .Select(detail =>
                    {
                        var startTimeText = detail.StartTime.HasValue
                            ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                            : "Chua xac dinh";
                        return $"tiec #{detail.OrderDetailId} luc {startTimeText}";
                    }));
            var subject = $"[Bookfet] Đơn hàng #{orderId} đã hoàn thành";
            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:16px;background:#f8fafc;color:#0f172a;"">
  <div style=""background:#ffffff;border-radius:12px;padding:20px;border:1px solid #e2e8f0;"">
    <h2 style=""margin:0 0 12px 0;"">Đơn hàng đã hoàn thành</h2>
    <p style=""margin:0 0 10px 0;"">Xin chào <strong>{customerName}</strong>,</p>
    <p style=""margin:0 0 12px 0;"">Bookfet trân trọng thông báo đơn hàng <strong>#{orderId}</strong> của bạn đã hoàn tất thanh toán qua <strong>{paymentMethodLabel}</strong> và được ghi nhận hoàn thành toàn bộ dịch vụ.</p>
    <p style=""margin:0 0 14px 0;"">
      Trạng thái:
      <span style=""display:inline-block;padding:4px 10px;border-radius:999px;background:#dcfce7;color:#16a34a;font-weight:700;"">
        HOÀN THÀNH
      </span>
    </p>
    {partySection}
    <p style=""margin:0 0 10px 0;"">Đội ngũ vận hành đã kết thúc các hạng mục của tiệc theo kế hoạch, đồng thời hệ thống đã cập nhật dữ liệu thanh toán và trạng thái đơn hàng của bạn.</p>
    <p style=""margin:0 0 10px 0;"">Nếu bạn cần hỗ trợ thêm về hóa đơn, lịch sử đơn hàng hoặc muốn đặt tiệc mới, vui lòng liên hệ Bookfet qua kênh chat hoặc hotline để được phục vụ nhanh nhất.</p>
    <p style=""margin:0;"">Bookfet cảm ơn bạn đã đồng hành và rất mong được tiếp tục phục vụ trong các sự kiện sắp tới.</p>
  </div>
</div>";
            var plainText =
                $"Xin chao {customerName}. Bookfet tran trong thong bao don hang #{orderId} da hoan tat thanh toan qua {paymentMethodLabel} va chuyen sang trang thai HOAN THANH. Cac tiec da duoc to chuc: {partyPlainText}. Neu ban can ho tro them, vui long lien he Bookfet qua kenh chat hoac hotline.";

            try
            {
                await _emailService.SendAsync(toEmail, subject, htmlBody, plainText);
            }
            catch
            {
                _logger.LogWarning("Failed to send completed-order email for orderId={OrderId}", orderId);
            }
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
                // Fallback below when image JSON is malformed.
            }

            return trimmed;
        }
    }
}
