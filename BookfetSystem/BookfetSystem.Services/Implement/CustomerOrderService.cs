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
        private readonly OrderRepository _orderRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly OrderServiceRepository _orderServiceRepository;
        private readonly ServiceRepository _serviceRepository;
        private readonly UserRepository _userRepository;
        private readonly MenuRepository _menuRepository;

        public CustomerOrderService(OrderRepository orderRepository, UserRepository userRepository, OrderDetailRepository orderDetailRepository, OrderServiceRepository orderServiceRepository, ServiceRepository serviceRepository, MenuRepository menuRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderServiceRepository = orderServiceRepository;
            _serviceRepository = serviceRepository;
            _menuRepository = menuRepository;
        }

        public async Task<PagedResponse<OrderResponse>> GetAllFilteredAsync(OrderFilterRequest filter, int page, int pageSize)
        {
            var entityFilter = filter.Adapt<Order>();
            entityFilter.Status = filter.Status?.ToString();

            var query = _orderRepository.GetAllOrderFiltered(entityFilter);
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
            var entity = await _orderRepository.GetByIdWithRelationAsync(id);

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
                await _orderRepository.CreateAsync(entity);
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
            var entity = await _orderRepository.GetByIdAsync(id);

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
                await _orderRepository.UpdateAsync(entity);
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
            var entity = await _orderRepository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = false
                };
            }

            var hasRelatedData = await _orderRepository.HasRelatedDataAsync(id);
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
                await _orderRepository.RemoveAsync(entity);
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

        public async Task<ApiResponse<int>> CreateOrderAsync(CreateOrderRequest request)
        {
            // create order
            var order = new Order
            {
                CustomerId = request.CustomerId,
                Status = OrderStatus.PENDING.ToString(),
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0
            };

            await _orderRepository.CreateAsync(order);

            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            var menuPrice = menu?.BasePrice ?? 0;

            // create order detail (TotalPrice from menu)
            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                Address = request.Address ?? string.Empty,
                NumberOfGuests = request.NumberOfGuests,
                Status = OrderStatus.PENDING.ToString(),
                TotalPrice = menuPrice,
                Type = "Order",
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MenuId = request.MenuId,
                PartyCategoryId = request.PartyCategoryId
            };

            await _orderDetailRepository.CreateAsync(orderDetail);

            decimal totalServicePrice = 0;

            // create order services
            if (request.Services != null && request.Services.Any())
            {
                foreach (var item in request.Services)
                {
                    var service = await _serviceRepository.GetByIdAsync(item.ServiceId);

                    if (service == null)
                        continue;

                    var orderService = new OrderService
                    {
                        OrderDetailId = orderDetail.OrderDetailId,
                        ServiceId = item.ServiceId,
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _orderServiceRepository.CreateAsync(orderService);

                    if (service.BasePrice != null)
                        totalServicePrice += service.BasePrice.Value * item.Quantity;
                }
            }

            await _orderServiceRepository.SaveAsync();

            // update order total = orderDetail (menu) + services
            order.TotalPrice = menuPrice + totalServicePrice;

            await _orderRepository.UpdateAsync(order);

            return new ApiResponse<int>
            {
                Success = true,
                Message = "Order created successfully.",
                Data = order.OrderId
            };
        }
    }
}