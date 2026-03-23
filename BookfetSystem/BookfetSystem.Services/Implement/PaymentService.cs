using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using System;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BookfetSystem.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly GSP26SE10DBContext _dbContext;
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public PaymentService(
            GSP26SE10DBContext dbContext,
            PaymentRepository paymentRepository,
            OrderRepository orderRepository,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _dbContext = dbContext;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<PagedResponse<PaymentResponse>> GetAllPaymentFilteredAsync(PaymentFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Payment>();
            var query = _paymentRepository.GetAllPaymentFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<PaymentResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<PaymentResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PaymentResponse>> CreateAsync(PaymentCreateRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<PaymentResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            var entity = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                PaymentType = request.PaymentType.ToString(),
                PaymentMethod = request.PaymentMethod.ToString(),
                PaymentStatus = BookfetSystem.Services.Enum.PaymentStatus.UNPAID.ToString(),
                PaidAt = DateTime.UtcNow
            };

            var affected = await _paymentRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<PaymentResponse>();
                return new ApiResponse<PaymentResponse>
                {
                    Success = true,
                    Message = "Payment created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PaymentResponse>
            {
                Success = false,
                Message = "Failed to create payment.",
                Data = null
            };
        }

        public async Task<ApiResponse<PaymentResponse>> UpdateAsync(int id, PaymentUpdateRequest request)
        {
            var entity = await _paymentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<PaymentResponse>
                {
                    Success = false,
                    Message = "Payment not found.",
                    Data = null
                };
            }

            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return new ApiResponse<PaymentResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            entity.OrderId = request.OrderId;
            entity.Amount = request.Amount;
            entity.PaymentType = request.PaymentType.ToString();
            entity.PaymentMethod = request.PaymentMethod.ToString();
            if (request.PaymentStatus.HasValue)
            {
                entity.PaymentStatus = request.PaymentStatus.Value.ToString();
            }
            entity.PaidAt = request.PaidAt;

            var affected = await _paymentRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<PaymentResponse>();
                return new ApiResponse<PaymentResponse>
                {
                    Success = true,
                    Message = "Payment updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PaymentResponse>
            {
                Success = false,
                Message = "Failed to update payment.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _paymentRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Payment not found.",
                    Data = false
                };
            }

            var removed = await _paymentRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Payment deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete payment.",
                Data = false
            };
        }

        public async Task<ApiResponse<object>> CreateDepositQR(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            var paymentCode = $"BOOKFET_{order.OrderId}";
            var qrBaseUrl = _configuration["SePay:QrBaseUrl"] ?? "https://qr.sepay.vn/img";
            var qrAccount = _configuration["SePay:QrAccountNumber"] ?? string.Empty;
            var qrBank = _configuration["SePay:QrBankCode"] ?? string.Empty;

            var existingUnpaid = await _paymentRepository.GetUnpaidDepositByOrderIdAsync(order.OrderId);
            if (existingUnpaid != null)
            {
                var amt = (int)Math.Round(existingUnpaid.Amount ?? 0);
                var url = $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amt}&des={Uri.EscapeDataString(paymentCode)}";

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "QR already exists for this order. Use existing payment.",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentCode,
                        amount = existingUnpaid.Amount,
                        qrUrl = url
                    }
                };
            }

            var depositAmount = order.TotalPrice * 0.5m;

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = depositAmount,
                PaymentType = PaymentType.DEPOSIT.ToString(),
                PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
                PaymentStatus = PaymentStatus.UNPAID.ToString(),
            };

            await _paymentRepository.CreateAsync(payment);

            var amountInt = (int)Math.Round(depositAmount ?? 0);
            var qrUrl =
                $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amountInt}&des={Uri.EscapeDataString(paymentCode)}";

            return new ApiResponse<object>
            {
                Success = true,
                Message = "QR created",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentCode,
                    amount = depositAmount,
                    qrUrl
                }
            };
        }

        public async Task<ApiResponse<object>> CreateFullQR(int orderId)
        {
            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            if (!string.Equals(order.Status, OrderStatus.BILLING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order must be BILLING before creating full payment QR."
                };
            }

            var remainingAmount = order.RemainingAmount ?? ((order.TotalPrice ?? 0m) - (order.DepositAmount ?? 0m));
            if (remainingAmount < 0)
            {
                remainingAmount = 0;
            }

            var extraChargeAmount = await _dbContext.OrderDetails
                .Where(x => x.OrderId == orderId)
                .SelectMany(x => x.OrderDetailExtraCharges)
                .SumAsync(x => x.TotalAmount) ?? 0m;

            var fullAmount = remainingAmount + extraChargeAmount;
            if (fullAmount <= 0)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order has no remaining amount to pay."
                };
            }

            var hasPaidFull = await _dbContext.Payments.AnyAsync(x =>
                x.OrderId == order.OrderId &&
                x.PaymentType == PaymentType.FULL.ToString() &&
                x.PaymentStatus == PaymentStatus.PAID.ToString());
            if (hasPaidFull)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order is already fully paid."
                };
            }

            var paymentCode = $"BOOKFET_FULL_{order.OrderId}";
            var qrBaseUrl = _configuration["SePay:QrBaseUrl"] ?? "https://qr.sepay.vn/img";
            var qrAccount = _configuration["SePay:QrAccountNumber"] ?? string.Empty;
            var qrBank = _configuration["SePay:QrBankCode"] ?? string.Empty;

            var existingUnpaid = await _paymentRepository.GetUnpaidByOrderIdAndTypeAsync(order.OrderId, PaymentType.FULL.ToString());
            if (existingUnpaid != null)
            {
                if ((existingUnpaid.Amount ?? 0m) != fullAmount)
                {
                    existingUnpaid.Amount = fullAmount;
                    await _paymentRepository.UpdateAsync(existingUnpaid);
                }

                var amt = (int)Math.Round(fullAmount);
                var url = $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amt}&des={Uri.EscapeDataString(paymentCode)}";

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "QR already exists for this order. Use existing full payment.",
                    Data = new
                    {
                        orderId = order.OrderId,
                        paymentCode,
                        remainingAmount,
                        extraChargeAmount,
                        amount = fullAmount,
                        qrUrl = url
                    }
                };
            }

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = fullAmount,
                PaymentType = PaymentType.FULL.ToString(),
                PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
                PaymentStatus = PaymentStatus.UNPAID.ToString(),
            };

            await _paymentRepository.CreateAsync(payment);

            var amountInt = (int)Math.Round(fullAmount);
            var qrUrl = $"{qrBaseUrl}?acc={Uri.EscapeDataString(qrAccount)}&bank={Uri.EscapeDataString(qrBank)}&amount={amountInt}&des={Uri.EscapeDataString(paymentCode)}";

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Full QR created",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentCode,
                    remainingAmount,
                    extraChargeAmount,
                    amount = fullAmount,
                    qrUrl
                }
            };
        }

        public async Task<ApiResponse<object>> CreateFullCashPayment(int orderId)
        {
            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);

            if (order == null)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order not found"
                };
            }

            if (!string.Equals(order.Status, OrderStatus.BILLING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order must be BILLING before creating full cash payment."
                };
            }

            var hasPaidFull = await _dbContext.Payments.AnyAsync(x =>
                x.OrderId == order.OrderId &&
                x.PaymentType == PaymentType.FULL.ToString() &&
                x.PaymentStatus == PaymentStatus.PAID.ToString());
            if (hasPaidFull)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order is already fully paid."
                };
            }

            var remainingAmount = order.RemainingAmount ?? ((order.TotalPrice ?? 0m) - (order.DepositAmount ?? 0m));
            if (remainingAmount < 0)
            {
                remainingAmount = 0;
            }

            var extraChargeAmount = await _dbContext.OrderDetails
                .Where(x => x.OrderId == orderId)
                .SelectMany(x => x.OrderDetailExtraCharges)
                .SumAsync(x => x.TotalAmount) ?? 0m;

            var fullAmount = remainingAmount + extraChargeAmount;
            if (fullAmount <= 0)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Order has no remaining amount to pay."
                };
            }

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = fullAmount,
                PaymentType = PaymentType.FULL.ToString(),
                PaymentMethod = PaymentMethod.CASH.ToString(),
                PaymentStatus = PaymentStatus.PAID.ToString(),
                PaidAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);

            order.RemainingAmount = 0;
            order.Status = OrderStatus.COMPLETED.ToString();
            await _orderRepository.UpdateAsync(order);
            await SendOrderCompletedEmailAsync(order, "tiền mặt");

            return new ApiResponse<object>
            {
                Success = true,
                Message = "Full cash payment created successfully.",
                Data = new
                {
                    orderId = order.OrderId,
                    paymentId = payment.PaymentId,
                    remainingAmount,
                    extraChargeAmount,
                    amount = fullAmount,
                    paymentType = payment.PaymentType,
                    paymentMethod = payment.PaymentMethod,
                    paymentStatus = payment.PaymentStatus,
                    paidAt = payment.PaidAt
                }
            };
        }

        private async Task SendOrderCompletedEmailAsync(Order order, string paymentMethodLabel)
        {
            var toEmail = order.Customer?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var customerName = string.IsNullOrWhiteSpace(order.Customer?.FullName)
                ? "Quý khách"
                : order.Customer!.FullName;
            var partyCards = (order.OrderDetails ?? new List<OrderDetail>())
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
                (order.OrderDetails ?? new List<OrderDetail>())
                    .OrderBy(x => x.StartTime)
                    .Select(detail =>
                    {
                        var startTimeText = detail.StartTime.HasValue
                            ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                            : "Chua xac dinh";
                        return $"tiec #{detail.OrderDetailId} luc {startTimeText}";
                    }));

            var subject = $"[Bookfet] Đơn hàng #{order.OrderId} đã hoàn thành";
            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:16px;background:#f8fafc;color:#0f172a;"">
  <div style=""background:#ffffff;border-radius:12px;padding:20px;border:1px solid #e2e8f0;"">
    <h2 style=""margin:0 0 12px 0;"">Đơn hàng đã hoàn thành</h2>
    <p style=""margin:0 0 10px 0;"">Xin chào <strong>{customerName}</strong>,</p>
    <p style=""margin:0 0 12px 0;"">Bookfet trân trọng thông báo đơn hàng <strong>#{order.OrderId}</strong> của bạn đã hoàn tất thanh toán qua <strong>{paymentMethodLabel}</strong> và được ghi nhận hoàn thành toàn bộ dịch vụ.</p>
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
                $"Xin chao {customerName}. Bookfet tran trong thong bao don hang #{order.OrderId} da hoan tat thanh toan qua {paymentMethodLabel} va chuyen sang trang thai HOAN THANH. Cac tiec da duoc to chuc: {partyPlainText}. Neu ban can ho tro them, vui long lien he Bookfet qua kenh chat hoac hotline.";

            try
            {
                await _emailService.SendAsync(toEmail, subject, htmlBody, plainText);
            }
            catch
            {
                // Email failure must not break payment completion flow.
            }
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
