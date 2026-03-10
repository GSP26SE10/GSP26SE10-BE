using BookfetSystem.Repositories;
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
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;

        public PaymentService(PaymentRepository paymentRepository, OrderRepository orderRepository, IConfiguration configuration)
        {
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

        public async Task<ApiResponse<object>> CreatePaymentQR(int orderId)
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

            var depositAmount = order.TotalPrice * 0.3m;

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = depositAmount,
                PaymentType = PaymentType.DEPOSIT.ToString(),
                PaymentMethod = PaymentMethod.BANK_TRANSFER.ToString(),
                PaymentStatus = PaymentStatus.UNPAID.ToString(),
            };

            await _paymentRepository.CreateAsync(payment);

            var paymentCode = $"BOOKFET_{order.OrderId}";

            var qrBaseUrl = _configuration["SePay:QrBaseUrl"] ?? "https://qr.sepay.vn/img";
            var qrAccount = _configuration["SePay:QrAccountNumber"] ?? string.Empty;
            var qrBank = _configuration["SePay:QrBankCode"] ?? string.Empty;

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
    }
}
