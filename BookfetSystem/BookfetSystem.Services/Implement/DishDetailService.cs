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
    public class DishDetailService : IDishDetailService
    {
        private readonly DishDetailRepository _dishDetailRepository;
        private readonly DishRepository _dishRepository;
        private readonly IngredientRepository _ingredientRepository;

        public DishDetailService(
            DishDetailRepository dishDetailRepository,
            DishRepository dishRepository,
            IngredientRepository ingredientRepository)
        {
            _dishDetailRepository = dishDetailRepository;
            _dishRepository = dishRepository;
            _ingredientRepository = ingredientRepository;
        }

        public async Task<PagedResponse<DishDetailResponse>> GetAllDishDetailFilteredAsync(DishDetailFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<DishDetail>();
            var query = _dishDetailRepository.GetAllDishDetailFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<DishDetailResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<DishDetailResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<DishDetailResponse>> CreateAsync(DishDetailCreateRequest request)
        {
            var dish = await _dishRepository.GetByIdAsync(request.DishId);
            if (dish == null)
            {
                return new ApiResponse<DishDetailResponse>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = null
                };
            }

            var ingredient = await _ingredientRepository.GetByIdAsync(request.IngredientId);
            if (ingredient == null)
            {
                return new ApiResponse<DishDetailResponse>
                {
                    Success = false,
                    Message = "Ingredient not found.",
                    Data = null
                };
            }

            if (await _dishDetailRepository.ExistsAsync(request.DishId, request.IngredientId))
            {
                return new ApiResponse<DishDetailResponse>
                {
                    Success = false,
                    Message = "This ingredient already exists in the selected dish.",
                    Data = null
                };
            }

            var entity = new DishDetail
            {
                DishId = request.DishId,
                IngredientId = request.IngredientId,
                Quantity = request.Quantity,
                Unit = request.Unit?.Trim()
            };

            var affected = await _dishDetailRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = await _dishDetailRepository
                    .GetAllDishDetailFiltered(new DishDetail { DishDetailId = entity.DishDetailId })
                    .ProjectToType<DishDetailResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<DishDetailResponse>
                {
                    Success = true,
                    Message = "Dish detail created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<DishDetailResponse>
            {
                Success = false,
                Message = "Failed to create dish detail.",
                Data = null
            };
        }

        public async Task<ApiResponse<DishDetailResponse>> UpdateAsync(int id, DishDetailUpdateRequest request)
        {
            var entity = await _dishDetailRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<DishDetailResponse>
                {
                    Success = false,
                    Message = "Dish detail not found.",
                    Data = null
                };
            }

            var dish = await _dishRepository.GetByIdAsync(request.DishId);
            if (dish == null)
            {
                return new ApiResponse<DishDetailResponse>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = null
                };
            }

            var ingredient = await _ingredientRepository.GetByIdAsync(request.IngredientId);
            if (ingredient == null)
            {
                return new ApiResponse<DishDetailResponse>
                {
                    Success = false,
                    Message = "Ingredient not found.",
                    Data = null
                };
            }

            if (await _dishDetailRepository.ExistsAsync(request.DishId, request.IngredientId, id))
            {
                return new ApiResponse<DishDetailResponse>
                {
                    Success = false,
                    Message = "This ingredient already exists in the selected dish.",
                    Data = null
                };
            }

            entity.DishId = request.DishId;
            entity.IngredientId = request.IngredientId;
            entity.Quantity = request.Quantity;
            entity.Unit = request.Unit?.Trim();

            var affected = await _dishDetailRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = await _dishDetailRepository
                    .GetAllDishDetailFiltered(new DishDetail { DishDetailId = entity.DishDetailId })
                    .ProjectToType<DishDetailResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<DishDetailResponse>
                {
                    Success = true,
                    Message = "Dish detail updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<DishDetailResponse>
            {
                Success = false,
                Message = "Failed to update dish detail.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _dishDetailRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Dish detail not found.",
                    Data = false
                };
            }

            var removed = await _dishDetailRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Dish detail deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete dish detail.",
                Data = false
            };
        }
    }
}
