using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Services.Implement
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly OrderDetailRepository _repository;
        private readonly OrderRepository _orderRepository;

        public OrderDetailService(OrderDetailRepository repository, OrderRepository orderRepository)
        {
            _repository = repository;
            _orderRepository = orderRepository;
        }

        public async Task<PagedResponse<OrderDetailResponse>> GetAllFilteredAsync(OrderDetailFilterRequest filter, int page, int pageSize)
        {
            var entityFilter = filter.Adapt<OrderDetail>();
            entityFilter.Status = filter.Status?.ToString();

            var query = _repository.GetAllOrderDetailFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var data = await query
                .ProjectToType<OrderDetailResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<OrderDetailResponse>
            {
                Items = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderDetailResponse>> Create(OrderDetailCreateRequest request)
        {
            if (!request.OrderId.HasValue)
            {
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "OrderId is required.",
                    Data = null
                };
            }

            var order = await _orderRepository.GetByIdAsync(request.OrderId.Value);
            if (order == null)
            {
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            var entity = new OrderDetail
            {
                OrderId = request.OrderId,
                Address = request.Address.Trim(),
                NumberOfGuests = request.NumberOfGuests,
                Status = (request.Status ?? OrderStatus.PENDING).ToString(),
                TotalPrice = request.TotalPrice,
                Type = request.Type.Trim(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                StaffGroupId = request.StaffGroupId,
                PartyCategoryId = request.PartyCategoryId,
                MenuId = request.MenuId
            };

            try
            {
                await _repository.CreateAsync(entity);
                var created = await GetById(entity.OrderDetailId);

                return new ApiResponse<OrderDetailResponse>
                {
                    Success = true,
                    Message = "Order detail created successfully.",
                    Data = created
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "Create order detail failed.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<OrderDetailResponse>> Update(int id, OrderDetailUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            if (request.OrderId.HasValue)
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId.Value);
                if (order == null)
                {
                    return new ApiResponse<OrderDetailResponse>
                    {
                        Success = false,
                        Message = "Order not found.",
                        Data = null
                    };
                }

                entity.OrderId = request.OrderId;
            }

            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                entity.Address = request.Address.Trim();
            }

            if (request.NumberOfGuests.HasValue)
            {
                entity.NumberOfGuests = request.NumberOfGuests;
            }

            if (request.Status.HasValue)
            {
                entity.Status = request.Status.Value.ToString();
            }

            if (request.TotalPrice.HasValue)
            {
                entity.TotalPrice = request.TotalPrice;
            }

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                entity.Type = request.Type.Trim();
            }

            if (request.StartTime.HasValue)
            {
                entity.StartTime = request.StartTime;
            }

            if (request.EndTime.HasValue)
            {
                entity.EndTime = request.EndTime;
            }

            if (request.StaffGroupId.HasValue)
            {
                entity.StaffGroupId = request.StaffGroupId;
            }

            if (request.PartyCategoryId.HasValue)
            {
                entity.PartyCategoryId = request.PartyCategoryId;
            }

            if (request.MenuId.HasValue)
            {
                entity.MenuId = request.MenuId;
            }

            if (string.IsNullOrWhiteSpace(entity.Status))
            {
                entity.Status = OrderStatus.PENDING.ToString();
            }

            try
            {
                await _repository.UpdateAsync(entity);
                var updated = await GetById(entity.OrderDetailId);

                return new ApiResponse<OrderDetailResponse>
                {
                    Success = true,
                    Message = "Order detail updated successfully.",
                    Data = updated
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<OrderDetailResponse>
                {
                    Success = false,
                    Message = "Update order detail failed.",
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
                    Message = "Order detail not found.",
                    Data = false
                };
            }

            try
            {
                await _repository.RemoveAsync(entity);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order detail deleted successfully.",
                    Data = true
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Delete order detail failed due to related data constraints.",
                    Data = false
                };
            }
        }

        public async Task<OrderDetailResponse?> GetById(int id)
        {
            var entity = await _repository.GetByIdWithRelationAsync(id);

            if (entity == null)
                return null;

            return entity.Adapt<OrderDetailResponse>();
        }
    }
}