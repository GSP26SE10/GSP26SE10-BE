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
    public class DishCategoryService : IDishCategoryService
    {
        private readonly DishCategoryRepository _dishCategoryRepository;

        public DishCategoryService(DishCategoryRepository dishCategoryRepository)
        {
            _dishCategoryRepository = dishCategoryRepository;
        }

        public async Task<PagedResponse<DishCategoryResponse>> GetAllDishCategoryFilteredAsync(DishCategoryFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<DishCategory>();
            var query = _dishCategoryRepository.GetAllDishCategoryFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<DishCategoryResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<DishCategoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<DishCategoryResponse>> CreateAsync(DishCategoryCreateRequest request)
        {
            var normalizedName = request.DishCategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<DishCategoryResponse>
                {
                    Success = false,
                    Message = "DishCategoryName is required.",
                    Data = null
                };
            }

            var exists = await _dishCategoryRepository
                .GetAllDishCategoryFiltered(new DishCategory { DishCategoryName = normalizedName })
                .AnyAsync(dc => dc.DishCategoryName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<DishCategoryResponse>
                {
                    Success = false,
                    Message = "DishCategoryName is existed.",
                    Data = null
                };
            }

            var entity = new DishCategory
            {
                DishCategoryName = normalizedName,
                Description = request.Description?.Trim()
            };

            var affected = await _dishCategoryRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<DishCategoryResponse>();
                return new ApiResponse<DishCategoryResponse>
                {
                    Success = true,
                    Message = "Dish category created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<DishCategoryResponse>
            {
                Success = false,
                Message = "Failed to create dish category.",
                Data = null
            };
        }

        public async Task<ApiResponse<DishCategoryResponse>> UpdateAsync(int id, DishCategoryUpdateRequest request)
        {
            var entity = await _dishCategoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<DishCategoryResponse>
                {
                    Success = false,
                    Message = "Dish category not found.",
                    Data = null
                };
            }

            var normalizedName = request.DishCategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<DishCategoryResponse>
                {
                    Success = false,
                    Message = "DishCategoryName is required.",
                    Data = null
                };
            }

            var exists = await _dishCategoryRepository
                .GetAllDishCategoryFiltered(new DishCategory { DishCategoryName = normalizedName })
                .AnyAsync(dc => dc.DishCategoryId != id && dc.DishCategoryName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<DishCategoryResponse>
                {
                    Success = false,
                    Message = "DishCategoryName is existed.",
                    Data = null
                };
            }

            entity.DishCategoryName = normalizedName;
            entity.Description = request.Description?.Trim();

            var affected = await _dishCategoryRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<DishCategoryResponse>();
                return new ApiResponse<DishCategoryResponse>
                {
                    Success = true,
                    Message = "Dish category updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<DishCategoryResponse>
            {
                Success = false,
                Message = "Failed to update dish category.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _dishCategoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Dish category not found.",
                    Data = false
                };
            }

            if (await _dishCategoryRepository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete dish category because it is being used in related data.",
                    Data = false
                };
            }

            var removed = await _dishCategoryRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Dish category deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete dish category.",
                Data = false
            };
        }
    }
}
