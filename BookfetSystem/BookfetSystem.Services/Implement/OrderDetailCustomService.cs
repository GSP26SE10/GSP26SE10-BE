using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class OrderDetailCustomService : IOrderDetailCustomService
    {
        private readonly OrderDetailCustomRepository _orderDetailCustomRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly DishRepository _dishRepository;

        public OrderDetailCustomService(
            OrderDetailCustomRepository orderDetailCustomRepository,
            OrderDetailRepository orderDetailRepository,
            DishRepository dishRepository)
        {
            _orderDetailCustomRepository = orderDetailCustomRepository;
            _orderDetailRepository = orderDetailRepository;
            _dishRepository = dishRepository;
        }

        public async Task<PagedResponse<OrderDetailCustomResponse>> GetAllOrderDetailCustomFilteredAsync(OrderDetailCustomFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<OrderDetailCustom>();
            var query = _orderDetailCustomRepository.GetAllOrderDetailCustomFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<OrderDetailCustomResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<OrderDetailCustomResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderDetailCustomResponse>> CreateAsync(OrderDetailCustomCreateRequest request)
        {
            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderDetailCustomResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            var dish = await _dishRepository.GetByIdAsync(request.DishId);
            if (dish == null)
            {
                return new ApiResponse<OrderDetailCustomResponse>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = null
                };
            }

            var entity = new OrderDetailCustom
            {
                OrderDetailId = request.OrderDetailId,
                DishId = request.DishId,
                Quantity = request.Quantity,
                TotalAmount = request.TotalAmount
            };

            var affected = await _orderDetailCustomRepository.CreateAsync(entity);
            if (affected > 0)
            {
                entity.Dish = dish;
                entity.OrderDetail = orderDetail;
                var response = entity.Adapt<OrderDetailCustomResponse>();

                return new ApiResponse<OrderDetailCustomResponse>
                {
                    Success = true,
                    Message = "Order detail custom created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderDetailCustomResponse>
            {
                Success = false,
                Message = "Failed to create order detail custom.",
                Data = null
            };
        }

        public async Task<ApiResponse<OrderDetailCustomResponse>> UpdateAsync(int id, OrderDetailCustomUpdateRequest request)
        {
            var entity = await _orderDetailCustomRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<OrderDetailCustomResponse>
                {
                    Success = false,
                    Message = "Order detail custom not found.",
                    Data = null
                };
            }

            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderDetailCustomResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            var dish = await _dishRepository.GetByIdAsync(request.DishId);
            if (dish == null)
            {
                return new ApiResponse<OrderDetailCustomResponse>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = null
                };
            }

            entity.OrderDetailId = request.OrderDetailId;
            entity.DishId = request.DishId;
            entity.Quantity = request.Quantity;
            entity.TotalAmount = request.TotalAmount;

            var affected = await _orderDetailCustomRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                entity.Dish = dish;
                entity.OrderDetail = orderDetail;
                var response = entity.Adapt<OrderDetailCustomResponse>();

                return new ApiResponse<OrderDetailCustomResponse>
                {
                    Success = true,
                    Message = "Order detail custom updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderDetailCustomResponse>
            {
                Success = false,
                Message = "Failed to update order detail custom.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _orderDetailCustomRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Order detail custom not found.",
                    Data = false
                };
            }

            var removed = await _orderDetailCustomRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order detail custom deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete order detail custom.",
                Data = false
            };
        }
    }
}
