using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class MenuCategoryService : IMenuCategoryService
    {
        private readonly MenuCategoryRepository _menuCategoryRepository;

        public MenuCategoryService(MenuCategoryRepository menuCategoryRepository)
        {
            _menuCategoryRepository = menuCategoryRepository;
        }

        public async Task<PagedResponse<MenuCategoryResponse>> GetAllMenuCategoryFilteredAsync(MenuCategoryFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<MenuCategory>();
            var query = _menuCategoryRepository.GetAllMenuCategoryFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<MenuCategoryResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<MenuCategoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<MenuCategoryResponse>> CreateAsync(MenuCategoryCreateRequest request)
        {
            var normalizedName = request.MenuCategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<MenuCategoryResponse>
                {
                    Success = false,
                    Message = "MenuCategoryName is required.",
                    Data = null
                };
            }

            var exists = await _menuCategoryRepository
                .GetAllMenuCategoryFiltered(new MenuCategory { MenuCategoryName = normalizedName })
                .AnyAsync(mc => mc.MenuCategoryName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<MenuCategoryResponse>
                {
                    Success = false,
                    Message = "MenuCategoryName is existed.",
                    Data = null
                };
            }

            var entity = new MenuCategory
            {
                MenuCategoryName = normalizedName,
                Description = request.Description?.Trim(),
                Status = MenuStatus.AVAILABLE.ToString()    
            };

            var affected = await _menuCategoryRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<MenuCategoryResponse>();
                return new ApiResponse<MenuCategoryResponse>
                {
                    Success = true,
                    Message = "Menu category created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<MenuCategoryResponse>
            {
                Success = false,
                Message = "Failed to create menu category.",
                Data = null
            };
        }

        public async Task<ApiResponse<MenuCategoryResponse>> UpdateAsync(int id, MenuCategoryUpdateRequest request)
        {
            var entity = await _menuCategoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<MenuCategoryResponse>
                {
                    Success = false,
                    Message = "Menu category not found.",
                    Data = null
                };
            }

            var normalizedName = request.MenuCategoryName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<MenuCategoryResponse>
                {
                    Success = false,
                    Message = "MenuCategoryName is required.",
                    Data = null
                };
            }

            var exists = await _menuCategoryRepository
                .GetAllMenuCategoryFiltered(new MenuCategory { MenuCategoryName = normalizedName })
                .AnyAsync(mc => mc.MenuCategoryId != id && mc.MenuCategoryName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<MenuCategoryResponse>
                {
                    Success = false,
                    Message = "MenuCategoryName is existed.",
                    Data = null
                };
            }

            entity.MenuCategoryName = normalizedName;
            entity.Description = request.Description?.Trim();
            entity.Status = request.Status.ToString();

            var affected = await _menuCategoryRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<MenuCategoryResponse>();
                return new ApiResponse<MenuCategoryResponse>
                {
                    Success = true,
                    Message = "Menu category updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<MenuCategoryResponse>
            {
                Success = false,
                Message = "Failed to update menu category.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _menuCategoryRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Menu category not found.",
                    Data = false
                };
            }

            if (await _menuCategoryRepository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete menu category because it is being used in related data.",
                    Data = false
                };
            }

            var removed = await _menuCategoryRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Menu category deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete menu category.",
                Data = false
            };
        }
    }
}
