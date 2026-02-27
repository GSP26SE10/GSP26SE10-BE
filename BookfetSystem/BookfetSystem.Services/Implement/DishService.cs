using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class DishService : IDishService
    {
        private readonly DishRepository _dishRepository;
        private readonly DishCategoryRepository _dishCategoryRepository;

        public DishService(DishRepository dishRepository, DishCategoryRepository dishCategoryRepository)
        {
            _dishRepository = dishRepository;
            _dishCategoryRepository = dishCategoryRepository;
        }

        public async Task<PagedResponse<DishResponse>> GetAllDishFilteredAsync(DishFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Dish>();
            var query = _dishRepository.GetAllDishFiltered(entityFilter, request.MinPrice, request.MaxPrice);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<DishResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<DishResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<DishResponse>> CreateAsync(DishCreateRequest request)
        {
            var normalizedName = request.DishName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<DishResponse>
                {
                    Success = false,
                    Message = "DishName is required.",
                    Data = null
                };
            }

            var exists = await _dishRepository
                .GetAllDishFiltered(new Dish { DishName = normalizedName }, null, null)
                .AnyAsync(d => d.DishName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<DishResponse>
                {
                    Success = false,
                    Message = "DishName is existed.",
                    Data = null
                };
            }

            if (request.DishCategoryId.HasValue)
            {
                var category = await _dishCategoryRepository.GetByIdAsync(request.DishCategoryId.Value);
                if (category == null)
                {
                    return new ApiResponse<DishResponse>
                    {
                        Success = false,
                        Message = "DishCategory not found.",
                        Data = null
                    };
                }
            }

            var entity = new Dish
            {
                DishName = normalizedName,
                Price = request.Price,
                Description = request.Description?.Trim(),
                Note = request.Note?.Trim(),
                Img = request.Img?.Trim(),
                Status = string.IsNullOrWhiteSpace(request.Status) ? "ACTIVE" : request.Status.Trim().ToUpper(),
                DishCategoryId = request.DishCategoryId
            };

            var affected = await _dishRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = await _dishRepository
                    .GetAllDishFiltered(new Dish { DishId = entity.DishId }, null, null)
                    .ProjectToType<DishResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<DishResponse>
                {
                    Success = true,
                    Message = "Dish created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<DishResponse>
            {
                Success = false,
                Message = "Failed to create dish.",
                Data = null
            };
        }

        public async Task<ApiResponse<DishResponse>> UpdateAsync(int id, DishUpdateRequest request)
        {
            var entity = await _dishRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<DishResponse>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = null
                };
            }

            var normalizedName = request.DishName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<DishResponse>
                {
                    Success = false,
                    Message = "DishName is required.",
                    Data = null
                };
            }

            var exists = await _dishRepository
                .GetAllDishFiltered(new Dish { DishName = normalizedName }, null, null)
                .AnyAsync(d => d.DishId != id && d.DishName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<DishResponse>
                {
                    Success = false,
                    Message = "DishName is existed.",
                    Data = null
                };
            }

            if (request.DishCategoryId.HasValue)
            {
                var category = await _dishCategoryRepository.GetByIdAsync(request.DishCategoryId.Value);
                if (category == null)
                {
                    return new ApiResponse<DishResponse>
                    {
                        Success = false,
                        Message = "DishCategory not found.",
                        Data = null
                    };
                }
            }

            entity.DishName = normalizedName;
            entity.Price = request.Price;
            entity.Description = request.Description?.Trim();
            entity.Note = request.Note?.Trim();
            entity.Img = request.Img?.Trim();
            entity.DishCategoryId = request.DishCategoryId;

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                entity.Status = request.Status.Trim().ToUpper();
            }

            var affected = await _dishRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = await _dishRepository
                    .GetAllDishFiltered(new Dish { DishId = entity.DishId }, null, null)
                    .ProjectToType<DishResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<DishResponse>
                {
                    Success = true,
                    Message = "Dish updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<DishResponse>
            {
                Success = false,
                Message = "Failed to update dish.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _dishRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = false
                };
            }

            if (await _dishRepository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete dish because it is being used in related data.",
                    Data = false
                };
            }

            var removed = await _dishRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Dish deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete dish.",
                Data = false
            };
        }
    }
}
