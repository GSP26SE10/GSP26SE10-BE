using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly OrderRepository _orderRepository;

        public OrderService(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<ApiResponse<OrderResponse>> CreateAsync(OrderCreateRequest request)
        {
            if (!await _orderRepository.CheckCustomerExist(request.CustomerId))
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            var entity = new Order
            {
                CustomerId = request.CustomerId,
                Status = "CREATE",
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0
            };

            var affected = await _orderRepository.CreateAsync(entity);

            if (affected > 0)
            {
                var order = await _orderRepository.GetOrderWithDetailAsync(entity.OrderId);
                var response = order!.Adapt<OrderResponse>();

                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderResponse>
            {
                Success = false,
                Message = "Failed to create order.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _orderRepository.GetByIdAsync(id);

            if (entity == null)
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = false
                };

            var removed = await _orderRepository.RemoveAsync(entity);

            return new ApiResponse<bool>
            {
                Success = removed,
                Message = removed ? "Deleted successfully." : "Delete failed.",
                Data = removed
            };
        }

        public async Task<PagedResponse<OrderResponse>> GetAllFilteredAsync(OrderFilterRequest request, int page, int pageSize)
        {
            var filter = request.Adapt<Order>();
            var query = _orderRepository.GetAllFiltered(filter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<OrderResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<OrderResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderResponse>> UpdateAsync(int id, OrderUpdateRequest request)
        {
            var entity = await _orderRepository.GetByIdAsync(id);

            if (entity == null)
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };

            entity.Status = request.Status;

            var affected = await _orderRepository.UpdateAsync(entity);

            if (affected > 0)
            {
                var order = await _orderRepository.GetOrderWithDetailAsync(id);
                var response = order!.Adapt<OrderResponse>();

                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderResponse>
            {
                Success = false,
                Message = "Update failed.",
                Data = null
            };
        }
    }
}