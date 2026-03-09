using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Services.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly OrderRepository _repository;
        private readonly UserRepository _userRepository;

        public CustomerOrderService(OrderRepository repository, UserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<PagedResponse<OrderResponse>> GetAllFilteredAsync(OrderFilterRequest filter, int page, int pageSize)
        {
            var entityFilter = filter.Adapt<Order>();
            entityFilter.Status = filter.Status?.ToString();

            var query = _repository.GetAllOrderFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var data = await query
                .ProjectToType<OrderResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<OrderResponse>
            {
                Items = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<OrderResponse?> GetById(int id)
        {
            var entity = await _repository.GetByIdWithRelationAsync(id);

            if (entity == null) return null;

            return new OrderResponse
            {
                OrderId = entity.OrderId,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer?.FullName,
                Status = entity.Status,
                TotalPrice = entity.TotalPrice,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<ApiResponse<OrderResponse>> Create(OrderCreateRequest request)
        {
            if (request.CustomerId.HasValue)
            {
                var customer = await _userRepository.GetByIdAsync(request.CustomerId.Value);
                if (customer == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Customer not found.",
                        Data = null
                    };
                }
            }

            var entity = new Order
            {
                CustomerId = request.CustomerId,
                Status = (request.Status ?? OrderStatus.PENDING).ToString(),
                TotalPrice = request.TotalPrice,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _repository.CreateAsync(entity);
                var created = await GetById(entity.OrderId);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order created successfully.",
                    Data = created
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Create order failed.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<OrderResponse>> Update(int id, OrderUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            if (request.CustomerId.HasValue)
            {
                var customer = await _userRepository.GetByIdAsync(request.CustomerId.Value);
                if (customer == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Customer not found.",
                        Data = null
                    };
                }

                entity.CustomerId = request.CustomerId;
            }

            if (request.Status.HasValue)
            {
                entity.Status = request.Status.Value.ToString();
            }

            if (request.TotalPrice.HasValue)
            {
                entity.TotalPrice = request.TotalPrice;
            }

            if (string.IsNullOrWhiteSpace(entity.Status))
            {
                entity.Status = OrderStatus.PENDING.ToString();
            }

            try
            {
                await _repository.UpdateAsync(entity);
                var updated = await GetById(entity.OrderId);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order updated successfully.",
                    Data = updated
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Update order failed.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = false
                };
            }

            var hasRelatedData = await _repository.HasRelatedDataAsync(id);
            if (hasRelatedData)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete order because it is referenced by payment/order detail records.",
                    Data = false
                };
            }

            try
            {
                await _repository.RemoveAsync(entity);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order deleted successfully.",
                    Data = true
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Delete order failed due to related data constraints.",
                    Data = false
                };
            }
        }
    }
}