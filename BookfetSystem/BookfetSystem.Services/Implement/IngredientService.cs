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
    public class IngredientService : IIngredientService
    {
        private readonly IngredientRepository _ingredientRepository;

        public IngredientService(IngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public async Task<PagedResponse<IngredientResponse>> GetAllIngredientFilteredAsync(IngredientFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Ingredient>();
            var query = _ingredientRepository.GetAllIngredientFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<IngredientResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<IngredientResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<IngredientResponse>> CreateAsync(IngredientCreateRequest request)
        {
            var normalizedName = request.IngredientName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<IngredientResponse>
                {
                    Success = false,
                    Message = "IngredientName is required.",
                    Data = null
                };
            }

            var exists = await _ingredientRepository
                .GetAllIngredientFiltered(new Ingredient { IngredientName = normalizedName })
                .AnyAsync(i => i.IngredientName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<IngredientResponse>
                {
                    Success = false,
                    Message = "IngredientName is existed.",
                    Data = null
                };
            }

            var entity = new Ingredient
            {
                IngredientName = normalizedName,
                Description = request.Description?.Trim(),
                Img = request.Img?.Trim()
            };

            var affected = await _ingredientRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<IngredientResponse>();
                return new ApiResponse<IngredientResponse>
                {
                    Success = true,
                    Message = "Ingredient created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<IngredientResponse>
            {
                Success = false,
                Message = "Failed to create ingredient.",
                Data = null
            };
        }

        public async Task<ApiResponse<IngredientResponse>> UpdateAsync(int id, IngredientUpdateRequest request)
        {
            var entity = await _ingredientRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<IngredientResponse>
                {
                    Success = false,
                    Message = "Ingredient not found.",
                    Data = null
                };
            }

            var normalizedName = request.IngredientName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<IngredientResponse>
                {
                    Success = false,
                    Message = "IngredientName is required.",
                    Data = null
                };
            }

            var exists = await _ingredientRepository
                .GetAllIngredientFiltered(new Ingredient { IngredientName = normalizedName })
                .AnyAsync(i => i.IngredientId != id && i.IngredientName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<IngredientResponse>
                {
                    Success = false,
                    Message = "IngredientName is existed.",
                    Data = null
                };
            }

            entity.IngredientName = normalizedName;
            entity.Description = request.Description?.Trim();
            entity.Img = request.Img?.Trim();

            var affected = await _ingredientRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<IngredientResponse>();
                return new ApiResponse<IngredientResponse>
                {
                    Success = true,
                    Message = "Ingredient updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<IngredientResponse>
            {
                Success = false,
                Message = "Failed to update ingredient.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _ingredientRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ingredient not found.",
                    Data = false
                };
            }

            if (await _ingredientRepository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete ingredient because it is being used in related data.",
                    Data = false
                };
            }

            var removed = await _ingredientRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Ingredient deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete ingredient.",
                Data = false
            };
        }
    }
}
