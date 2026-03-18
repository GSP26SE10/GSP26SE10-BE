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

namespace BookfetSystem.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly GSP26SE10DBContext _dbContext;
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;

        public PaymentService(
            GSP26SE10DBContext dbContext,
            PaymentRepository paymentRepository,
            OrderRepository orderRepository,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
            _configuration = configuration;
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
    }
}
