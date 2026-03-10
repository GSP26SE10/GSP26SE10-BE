using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class OrderServiceManager : IOrderServiceManager
    {
        private readonly OrderServiceRepository _orderServiceRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly ServiceRepository _serviceRepository;

        public OrderServiceManager(
            OrderServiceRepository orderServiceRepository,
            OrderDetailRepository orderDetailRepository,
            ServiceRepository serviceRepository)
        {
            _orderServiceRepository = orderServiceRepository;
            _orderDetailRepository = orderDetailRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<PagedResponse<OrderServiceResponse>> GetAllOrderServiceFilteredAsync(OrderServiceFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<OrderService>();
            var query = _orderServiceRepository.GetAllOrderServiceFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<OrderServiceResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<OrderServiceResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderServiceResponse>> CreateAsync(OrderServiceCreateRequest request)
        {
            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderServiceResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<OrderServiceResponse>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = null
                };
            }

            var entity = new OrderService
            {
                OrderDetailId = request.OrderDetailId,
                ServiceId = request.ServiceId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            var affected = await _orderServiceRepository.CreateAsync(entity);
            if (affected > 0)
            {
                entity.Service = service;
                entity.OrderDetail = orderDetail;
                var response = entity.Adapt<OrderServiceResponse>();

                return new ApiResponse<OrderServiceResponse>
                {
                    Success = true,
                    Message = "Order service created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderServiceResponse>
            {
                Success = false,
                Message = "Failed to create order service.",
                Data = null
            };
        }

        public async Task<ApiResponse<OrderServiceResponse>> UpdateAsync(int id, OrderServiceUpdateRequest request)
        {
            var entity = await _orderServiceRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<OrderServiceResponse>
                {
                    Success = false,
                    Message = "Order service not found.",
                    Data = null
                };
            }

            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderServiceResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<OrderServiceResponse>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = null
                };
            }

            entity.OrderDetailId = request.OrderDetailId;
            entity.ServiceId = request.ServiceId;
            entity.Quantity = request.Quantity;

            var affected = await _orderServiceRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                entity.Service = service;
                entity.OrderDetail = orderDetail;
                var response = entity.Adapt<OrderServiceResponse>();

                return new ApiResponse<OrderServiceResponse>
                {
                    Success = true,
                    Message = "Order service updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderServiceResponse>
            {
                Success = false,
                Message = "Failed to update order service.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _orderServiceRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Order service not found.",
                    Data = false
                };
            }

            var removed = await _orderServiceRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order service deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete order service.",
                Data = false
            };
        }
    }
}
