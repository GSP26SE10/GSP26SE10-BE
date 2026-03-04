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
    public class FeedbackMenuService : IFeedbackMenuService
    {
        private readonly FeedbackMenuRepository _feedbackMenuRepository;
        private readonly MenuRepository _menuRepository;
        private readonly UserRepository _userRepository;

        public FeedbackMenuService(
            FeedbackMenuRepository feedbackMenuRepository,
            MenuRepository menuRepository,
            UserRepository userRepository)
        {
            _feedbackMenuRepository = feedbackMenuRepository;
            _menuRepository = menuRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResponse<FeedbackMenuResponse>> GetAllFeedbackMenuFilteredAsync(FeedbackMenuFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<FeedbackMenu>();
            entityFilter.Status = request.Status?.ToString();
            entityFilter.Rating = request.Rating ?? 0;

            var query = _feedbackMenuRepository.GetAllFeedbackMenuFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<FeedbackMenuResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<FeedbackMenuResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<FeedbackMenuResponse>> CreateAsync(FeedbackMenuCreateRequest request)
        {
            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            var entity = new FeedbackMenu
            {
                MenuId = request.MenuId,
                CustomerId = request.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
                Status = FeedbackMenuStatus.ACTIVE.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            var affected = await _feedbackMenuRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = await _feedbackMenuRepository
                    .GetAllFeedbackMenuFiltered(new FeedbackMenu { FeedbackMenuId = entity.FeedbackMenuId })
                    .ProjectToType<FeedbackMenuResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = true,
                    Message = "Feedback menu created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<FeedbackMenuResponse>
            {
                Success = false,
                Message = "Failed to create feedback menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<FeedbackMenuResponse>> UpdateAsync(int id, FeedbackMenuUpdateRequest request)
        {
            var entity = await _feedbackMenuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Feedback menu not found.",
                    Data = null
                };
            }

            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = null
                };
            }

            entity.MenuId = request.MenuId;
            entity.CustomerId = request.CustomerId;
            entity.Rating = request.Rating;
            entity.Comment = request.Comment?.Trim();
            if (request.Status != null)
            {
                entity.Status = request.Status.ToString();
            }

            var affected = await _feedbackMenuRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = await _feedbackMenuRepository
                    .GetAllFeedbackMenuFiltered(new FeedbackMenu { FeedbackMenuId = entity.FeedbackMenuId })
                    .ProjectToType<FeedbackMenuResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<FeedbackMenuResponse>
                {
                    Success = true,
                    Message = "Feedback menu updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<FeedbackMenuResponse>
            {
                Success = false,
                Message = "Failed to update feedback menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _feedbackMenuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Feedback menu not found.",
                    Data = false
                };
            }

            var removed = await _feedbackMenuRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Feedback menu deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete feedback menu.",
                Data = false
            };
        }
    }
}
