using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookfetSystem.API.BackgroundJobs
{
    public interface IOrderStatusTransitionJob
    {
        Task MoveToInProgressAsync(int orderDetailId, DateTime expectedStartTimeUtc);
        Task MoveToCompletedAsync(int orderDetailId, DateTime expectedEndTimeUtc);
    }

    public interface IOrderDepositTimeoutJob
    {
        Task CancelOrderIfDepositUnpaidAsync(int orderId, DateTime expectedCreatedAtUtc);
    }

    public interface IOrderPendingApprovalAutoCancelJob
    {
        Task CancelPendingOrderIfStillUnapprovedAsync(int orderId, DateTime expectedFirstPartyStartUtc);
    }

    public class OrderStatusTransitionJob : IOrderStatusTransitionJob
    {
        private readonly GSP26SE10DBContext _dbContext;

        public OrderStatusTransitionJob(GSP26SE10DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task MoveToInProgressAsync(int orderDetailId, DateTime expectedStartTimeUtc)
        {
            var detail = await _dbContext.OrderDetails
                .FirstOrDefaultAsync(x => x.OrderDetailId == orderDetailId);
            if (detail == null || !detail.StartTime.HasValue)
            {
                return;
            }

            var actualStartUtc = ToUtc(detail.StartTime.Value);

            if (string.Equals(detail.Status, OrderDetailStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.CANCELLED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.REJECTED.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(detail.Status, OrderDetailStatus.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            detail.Status = OrderDetailStatus.IN_PROGRESS.ToString();
            await _dbContext.SaveChangesAsync();

            await RecalculateParentOrderStatusAsync(detail.OrderId);
        }

        public async Task MoveToCompletedAsync(int orderDetailId, DateTime expectedEndTimeUtc)
        {
            var detail = await _dbContext.OrderDetails
                .FirstOrDefaultAsync(x => x.OrderDetailId == orderDetailId);
            if (detail == null || !detail.EndTime.HasValue)
            {
                return;
            }

            var actualEndUtc = ToUtc(detail.EndTime.Value);

            if (string.Equals(detail.Status, OrderDetailStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.CANCELLED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.REJECTED.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            detail.Status = OrderDetailStatus.COMPLETED.ToString();
            await _dbContext.SaveChangesAsync();

            await RecalculateParentOrderStatusAsync(detail.OrderId);
        }

        private async Task RecalculateParentOrderStatusAsync(int? orderId)
        {
            if (!orderId.HasValue)
            {
                return;
            }

            var order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.OrderId == orderId.Value);

            if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return;
            }

            var detailStatuses = order.OrderDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.Status))
                .Select(x => x.Status!)
                .ToList();

            if (detailStatuses.Count == 0)
            {
                return;
            }

            var allCompleted = detailStatuses.All(x => string.Equals(x, OrderDetailStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase));
            if (allCompleted)
            {
                if (!string.Equals(order.Status, OrderStatus.BILLING.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    order.Status = OrderStatus.BILLING.ToString();
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                if (!string.Equals(order.Status, OrderStatus.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    order.Status = OrderStatus.IN_PROGRESS.ToString();
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        private static DateTime ToUtc(DateTime input)
        {
            return input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };
        }

    }

    public class OrderDepositTimeoutJob : IOrderDepositTimeoutJob
    {
        private readonly GSP26SE10DBContext _dbContext;

        public OrderDepositTimeoutJob(GSP26SE10DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CancelOrderIfDepositUnpaidAsync(int orderId, DateTime expectedCreatedAtUtc)
        {
            var order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null || !order.CreatedAt.HasValue)
            {
                return;
            }

            var orderCreatedAtUtc = ToUtc(order.CreatedAt.Value);
            // Ignore stale jobs that don't match the actual order creation timestamp.
            if (Math.Abs((orderCreatedAtUtc - expectedCreatedAtUtc).TotalSeconds) > 5)
            {
                return;
            }

            if (!string.Equals(order.Status, OrderStatus.PENDING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var hasPaidDeposit = order.Payments.Any(p =>
                string.Equals(p.PaymentType, PaymentType.DEPOSIT.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.PaymentStatus, PaymentStatus.PAID.ToString(), StringComparison.OrdinalIgnoreCase));

            if (hasPaidDeposit || (order.DepositAmount ?? 0) > 0)
            {
                return;
            }

            // Final check right before cancellation to reduce webhook race conditions.
            var hasPaidDepositFinal = await _dbContext.Payments.AnyAsync(p =>
                p.OrderId == orderId &&
                p.PaymentType == PaymentType.DEPOSIT.ToString() &&
                p.PaymentStatus == PaymentStatus.PAID.ToString());

            if (hasPaidDepositFinal)
            {
                return;
            }

            order.Status = OrderStatus.CANCELLED.ToString();
            foreach (var detail in order.OrderDetails)
            {
                detail.Status = OrderDetailStatus.CANCELLED.ToString();
            }

            await _dbContext.SaveChangesAsync();
        }

        private static DateTime ToUtc(DateTime input)
        {
            return input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };
        }
    }

    public class OrderPendingApprovalAutoCancelJob : IOrderPendingApprovalAutoCancelJob
    {
        private readonly GSP26SE10DBContext _dbContext;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;

        public OrderPendingApprovalAutoCancelJob(
            GSP26SE10DBContext dbContext,
            IPaymentService paymentService,
            IEmailService emailService)
        {
            _dbContext = dbContext;
            _paymentService = paymentService;
            _emailService = emailService;
        }

        public async Task CancelPendingOrderIfStillUnapprovedAsync(int orderId, DateTime expectedFirstPartyStartUtc)
        {
            var order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .Include(x => x.Payments)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return;
            }

            if (!string.Equals(order.Status, OrderStatus.PENDING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var firstPartyStart = order.OrderDetails
                .Where(x => x.StartTime.HasValue)
                .Select(x => x.StartTime!.Value)
                .OrderBy(x => x)
                .FirstOrDefault();

            if (firstPartyStart == default)
            {
                return;
            }

            var firstPartyStartUtc = ToUtc(firstPartyStart);
            // Ignore stale jobs when first party schedule has changed.
            if (Math.Abs((firstPartyStartUtc - expectedFirstPartyStartUtc).TotalSeconds) > 5)
            {
                return;
            }

            var hasPaidDeposit = order.Payments.Any(p =>
                string.Equals(p.PaymentType, PaymentType.DEPOSIT.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.PaymentStatus, PaymentStatus.PAID.ToString(), StringComparison.OrdinalIgnoreCase));

            if (!hasPaidDeposit && (order.DepositAmount ?? 0m) <= 0m)
            {
                return;
            }

            var latestPaidDeposit = order.Payments
                .Where(p =>
                    string.Equals(p.PaymentType, PaymentType.DEPOSIT.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.PaymentStatus, PaymentStatus.PAID.ToString(), StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PaidAt ?? DateTime.MinValue)
                .FirstOrDefault();

            var refundAmountForEmail = 0m;
            var refundCaseNote = "Đơn chưa ghi nhận tiền cọc, không phát sinh hoàn tiền.";
            if (latestPaidDeposit != null &&
                string.Equals(latestPaidDeposit.PaymentMethod, PaymentMethod.ZALOPAY.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var refundAmount = latestPaidDeposit.Amount ?? 0m;
                if (refundAmount > 0m)
                {
                    var refundResult = await _paymentService.RefundOrderDepositByAmountAsync(
                        order.OrderId,
                        refundAmount,
                        "Auto-cancel because order remained unapproved 2 days before the first party.");

                    if (!refundResult.Success)
                    {
                        return;
                    }

                    refundAmountForEmail = ExtractRefundAmount(refundResult);
                    if (refundAmountForEmail <= 0m)
                    {
                        refundAmountForEmail = refundAmount;
                    }
                    refundCaseNote = "Đơn đã được hoàn tiền cọc tự động qua ZaloPay vì chưa được duyệt trước mốc 2 ngày diễn ra tiệc đầu tiên.";
                }
            }
            else if (latestPaidDeposit != null)
            {
                var paymentMethodLabel = latestPaidDeposit.PaymentMethod ?? "UNKNOWN";
                refundCaseNote = $"Đơn cọc thanh toán qua phương thức {paymentMethodLabel}, hệ thống chỉ tự động hoàn cọc qua ZaloPay. Vui lòng liên hệ Bookfet để được hỗ trợ hoàn tiền thủ công.";
            }
            else if ((order.DepositAmount ?? 0m) > 0m)
            {
                refundCaseNote = "Đơn có ghi nhận tiền cọc nhưng không tìm thấy giao dịch ZaloPay hợp lệ để tự động hoàn tiền.";
            }

            order.Status = OrderStatus.CANCELLED.ToString();
            order.NoteOrder = "Auto-cancelled: order remained unapproved 2 days before the first party.";
            order.ReviewedAt = DateTime.UtcNow;

            foreach (var detail in order.OrderDetails)
            {
                detail.Status = OrderDetailStatus.CANCELLED.ToString();
            }

            await _dbContext.SaveChangesAsync();
            await SendAutoCancelledPendingApprovalEmailAsync(order, refundAmountForEmail, refundCaseNote);
        }

        private static DateTime ToUtc(DateTime input)
        {
            return input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };
        }

        private async Task SendAutoCancelledPendingApprovalEmailAsync(Repositories.Entities.Order order, decimal refundAmount, string refundCaseNote)
        {
            var toEmail = order.Customer?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var customerName = string.IsNullOrWhiteSpace(order.Customer?.FullName) ? "Quý khách" : order.Customer!.FullName;
            var detailCards = (order.OrderDetails ?? new List<Repositories.Entities.OrderDetail>())
                .OrderBy(x => x.StartTime)
                .Select(detail =>
                {
                    var startTimeText = detail.StartTime.HasValue
                        ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                        : "Chưa xác định";

                    var menuName = GetMenuName(detail);
                    var menuPrice = GetMenuBasePrice(detail);
                    var menuPriceText = menuPrice.HasValue ? FormatVnd(menuPrice.Value) : "Chưa cập nhật";
                    var menuImageUrl = GetMenuImageUrl(detail);
                    var imageHtml = string.IsNullOrWhiteSpace(menuImageUrl)
                        ? @"<div style=""height:120px;border-radius:10px;background:#e2e8f0;color:#334155;display:flex;align-items:center;justify-content:center;font-weight:600;"">Tiệc đơn giản</div>"
                        : $@"<img src=""{menuImageUrl}"" alt=""{menuName}"" style=""width:100%;height:120px;object-fit:cover;border-radius:10px;display:block;"" />";

                    var cardHtml = $@"
<div style=""border:1px solid #e2e8f0;border-radius:10px;padding:12px;margin-bottom:10px;background:#f8fafc;"">
  {imageHtml}
  <p style=""margin:10px 0 4px 0;font-weight:700;"">{menuName}</p>
  <p style=""margin:0;color:#334155;"">Giá menu: <strong>{menuPriceText}</strong>/khách</p>
  <p style=""margin:4px 0 0 0;color:#334155;"">Mã tiệc: <strong>#{detail.OrderDetailId}</strong></p>
  <p style=""margin:4px 0 0 0;color:#334155;"">Thời gian tổ chức: <strong>{startTimeText}</strong></p>
</div>";

                    var plainText = $"{menuName} (gia menu {menuPriceText}/khach, tiec #{detail.OrderDetailId}, thoi gian {startTimeText})";
                    return (cardHtml, plainText);
                })
                .ToList();

            var detailSection = detailCards.Count == 0
                ? string.Empty
                : $@"
<div style=""margin:14px 0;"">
  <p style=""margin:0 0 8px 0;font-weight:700;"">Danh sách tiệc đã đặt:</p>
  {string.Join(string.Empty, detailCards.Select(x => x.cardHtml))}
</div>";

            var detailPlainText = detailCards.Count == 0
                ? "khong co du lieu tiec"
                : string.Join("; ", detailCards.Select(x => x.plainText));

            var subject = $"[Bookfet] Đơn hàng #{order.OrderId} tự động hủy do chưa được duyệt";
            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:16px;background:#f8fafc;color:#0f172a;"">
  <div style=""background:#ffffff;border-radius:12px;padding:20px;border:1px solid #e2e8f0;"">
    <h2 style=""margin:0 0 12px 0;"">Thông báo tự động hủy đơn hàng</h2>
    <p style=""margin:0 0 10px 0;"">Xin chào <strong>{customerName}</strong>,</p>
    <p style=""margin:0 0 12px 0;"">Đơn hàng <strong>#{order.OrderId}</strong> đã được hệ thống tự động hủy vì đến mốc còn <strong>2 ngày trước tiệc đầu tiên</strong> nhưng đơn vẫn chưa được quản trị viên duyệt.</p>
    <p style=""margin:0 0 14px 0;"">
      Trạng thái:
      <span style=""display:inline-block;padding:4px 10px;border-radius:999px;background:#fee2e2;color:#dc2626;font-weight:700;"">
        ĐÃ HỦY
      </span>
    </p>
    {detailSection}
    <div style=""margin:12px 0 14px 0;padding:12px;border-radius:10px;background:#eff6ff;border:1px solid #bfdbfe;color:#1e3a8a;"">
      <p style=""margin:0 0 8px 0;font-weight:700;"">Thông tin hoàn tiền cọc</p>
      <p style=""margin:0;"">Số tiền hoàn tự động: <strong>{FormatVnd(refundAmount)}</strong></p>
      <p style=""margin:6px 0 0 0;"">Kết quả xử lý: <strong>{refundCaseNote}</strong></p>
    </div>
    <p style=""margin:0;"">Nếu bạn cần hỗ trợ thêm, vui lòng liên hệ Bookfet qua kênh chat hoặc hotline.</p>
  </div>
</div>";

            var plainText =
                $"Xin chao {customerName}. Don hang #{order.OrderId} da bi tu dong huy do den moc con 2 ngay truoc tiec dau tien nhung chua duoc duyet. Thong tin hoan coc: {FormatVnd(refundAmount)}. Ket qua: {refundCaseNote}. Cac tiec: {detailPlainText}.";

            try
            {
                await _emailService.SendAsync(toEmail, subject, htmlBody, plainText);
            }
            catch
            {
                // Email failure should not block auto-cancel flow.
            }
        }

        private static string GetMenuName(Repositories.Entities.OrderDetail detail)
        {
            if (TryReadMenuSnapshot(detail.MenuSnapshot, out var menuName, out _, out _))
            {
                return string.IsNullOrWhiteSpace(menuName) ? "Tiệc đơn giản" : menuName;
            }

            return string.IsNullOrWhiteSpace(detail.Menu?.MenuName) ? "Tiệc đơn giản" : detail.Menu!.MenuName;
        }

        private static decimal? GetMenuBasePrice(Repositories.Entities.OrderDetail detail)
        {
            if (TryReadMenuSnapshot(detail.MenuSnapshot, out _, out var basePrice, out _))
            {
                return basePrice;
            }

            return detail.Menu?.BasePrice;
        }

        private static string? GetMenuImageUrl(Repositories.Entities.OrderDetail detail)
        {
            if (TryReadMenuSnapshot(detail.MenuSnapshot, out _, out _, out var imageUrl))
            {
                return imageUrl;
            }

            return GetFirstImageUrl(detail.Menu?.ImgUrl);
        }

        private static bool TryReadMenuSnapshot(string? rawSnapshot, out string menuName, out decimal? basePrice, out string? firstImageUrl)
        {
            menuName = string.Empty;
            basePrice = null;
            firstImageUrl = null;

            if (string.IsNullOrWhiteSpace(rawSnapshot))
            {
                return false;
            }

            try
            {
                using var json = JsonDocument.Parse(rawSnapshot);
                var root = json.RootElement;

                if (root.TryGetProperty("MenuName", out var menuNameElement) && menuNameElement.ValueKind == JsonValueKind.String)
                {
                    menuName = menuNameElement.GetString() ?? string.Empty;
                }
                else if (root.TryGetProperty("menuName", out var menuNameElementCamel) && menuNameElementCamel.ValueKind == JsonValueKind.String)
                {
                    menuName = menuNameElementCamel.GetString() ?? string.Empty;
                }

                if (root.TryGetProperty("BasePrice", out var basePriceElement) && basePriceElement.ValueKind == JsonValueKind.Number && basePriceElement.TryGetDecimal(out var parsedBasePrice))
                {
                    basePrice = parsedBasePrice;
                }
                else if (root.TryGetProperty("basePrice", out var basePriceElementCamel) && basePriceElementCamel.ValueKind == JsonValueKind.Number && basePriceElementCamel.TryGetDecimal(out parsedBasePrice))
                {
                    basePrice = parsedBasePrice;
                }

                if (root.TryGetProperty("ImgUrl", out var imgElement) || root.TryGetProperty("imgUrl", out imgElement))
                {
                    firstImageUrl = ParseFirstImageUrl(imgElement);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string? ParseFirstImageUrl(JsonElement imgElement)
        {
            if (imgElement.ValueKind == JsonValueKind.String)
            {
                var raw = imgElement.GetString();
                return string.IsNullOrWhiteSpace(raw) ? null : raw;
            }

            if (imgElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in imgElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var raw = item.GetString();
                        if (!string.IsNullOrWhiteSpace(raw))
                        {
                            return raw;
                        }
                    }
                }
            }

            return null;
        }

        private static decimal ExtractRefundAmount(Services.Models.Common.ApiResponse<object> refundResult)
        {
            if (refundResult?.Data == null)
            {
                return 0m;
            }

            try
            {
                using var json = JsonDocument.Parse(JsonSerializer.Serialize(refundResult.Data));
                if (json.RootElement.TryGetProperty("amount", out var amountElement))
                {
                    if (amountElement.ValueKind == JsonValueKind.Number && amountElement.TryGetDecimal(out var amountDecimal))
                    {
                        return amountDecimal;
                    }

                    if (amountElement.ValueKind == JsonValueKind.String &&
                        decimal.TryParse(amountElement.GetString(), out amountDecimal))
                    {
                        return amountDecimal;
                    }
                }
            }
            catch
            {
                // Keep default fallback.
            }

            return 0m;
        }

        private static string FormatVnd(decimal amount) => $"{amount:N0} VND";

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
                // Keep fallback to raw string.
            }

            return trimmed;
        }
    }

    public class OrderStatusSchedulerService : IOrderStatusSchedulerService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public OrderStatusSchedulerService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Task ScheduleOrderDetailStatusTransitionsAsync(int orderDetailId, DateTime? startTime, DateTime? endTime)
        {
            if (startTime.HasValue)
            {
                var startUtc = ToUtc(startTime.Value);
                if (startUtc <= DateTime.UtcNow)
                {
                    _backgroundJobClient.Enqueue<IOrderStatusTransitionJob>(x => x.MoveToInProgressAsync(orderDetailId, startUtc));
                }
                else
                {
                    _backgroundJobClient.Schedule<IOrderStatusTransitionJob>(
                        x => x.MoveToInProgressAsync(orderDetailId, startUtc),
                        startUtc - DateTime.UtcNow);
                }
            }

            if (endTime.HasValue)
            {
                var endUtc = ToUtc(endTime.Value);
                if (endUtc <= DateTime.UtcNow)
                {
                    _backgroundJobClient.Enqueue<IOrderStatusTransitionJob>(x => x.MoveToCompletedAsync(orderDetailId, endUtc));
                }
                else
                {
                    _backgroundJobClient.Schedule<IOrderStatusTransitionJob>(
                        x => x.MoveToCompletedAsync(orderDetailId, endUtc),
                        endUtc - DateTime.UtcNow);
                }
            }

            return Task.CompletedTask;
        }

        public Task ScheduleOrderDepositTimeoutAsync(int orderId, DateTime? createdAt)
        {
            if (orderId <= 0 || !createdAt.HasValue)
            {
                return Task.CompletedTask;
            }

            var createdAtUtc = ToUtc(createdAt.Value);
            var executeAtUtc = createdAtUtc.AddMinutes(5);

            if (executeAtUtc <= DateTime.UtcNow)
            {
                _backgroundJobClient.Enqueue<IOrderDepositTimeoutJob>(
                    x => x.CancelOrderIfDepositUnpaidAsync(orderId, createdAtUtc));
            }
            else
            {
                _backgroundJobClient.Schedule<IOrderDepositTimeoutJob>(
                    x => x.CancelOrderIfDepositUnpaidAsync(orderId, createdAtUtc),
                    executeAtUtc - DateTime.UtcNow);
            }

            return Task.CompletedTask;
        }

        public Task SchedulePendingApprovalAutoCancelAsync(int orderId, DateTime? firstPartyStartTime)
        {
            if (orderId <= 0 || !firstPartyStartTime.HasValue)
            {
                return Task.CompletedTask;
            }

            var firstPartyStartUtc = ToUtc(firstPartyStartTime.Value);
            var executeAtUtc = firstPartyStartUtc.AddDays(-2);

            if (executeAtUtc <= DateTime.UtcNow)
            {
                _backgroundJobClient.Enqueue<IOrderPendingApprovalAutoCancelJob>(
                    x => x.CancelPendingOrderIfStillUnapprovedAsync(orderId, firstPartyStartUtc));
            }
            else
            {
                _backgroundJobClient.Schedule<IOrderPendingApprovalAutoCancelJob>(
                    x => x.CancelPendingOrderIfStillUnapprovedAsync(orderId, firstPartyStartUtc),
                    executeAtUtc - DateTime.UtcNow);
            }

            return Task.CompletedTask;
        }

        private static DateTime ToUtc(DateTime input)
        {
            return input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };
        }
    }
}
