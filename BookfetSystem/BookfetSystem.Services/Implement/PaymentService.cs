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

namespace BookfetSystem.Services.Implement
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderRepository _orderRepository;

        public PaymentService(PaymentRepository paymentRepository, OrderRepository orderRepository)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
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
    }
}
