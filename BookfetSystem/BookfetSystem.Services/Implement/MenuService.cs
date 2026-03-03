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
    public class MenuService : IMenuService
    {
        private readonly MenuRepository _menuRepository;
        private readonly MenuCategoryRepository _menuCategoryRepository;

        public MenuService(MenuRepository menuRepository, MenuCategoryRepository menuCategoryRepository)
        {
            _menuRepository = menuRepository;
            _menuCategoryRepository = menuCategoryRepository;
        }

        public async Task<PagedResponse<MenuResponse>> GetAllMenuFilteredAsync(MenuFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Menu>();
            var query = _menuRepository.GetAllMenuFiltered(entityFilter, request.MinBasePrice, request.MaxBasePrice);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<MenuResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<MenuResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<MenuResponse>> CreateAsync(MenuCreateRequest request)
        {
            var normalizedName = request.MenuName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuName is required.",
                    Data = null
                };
            }

            var menuCategory = await _menuCategoryRepository.GetByIdAsync(request.MenuCategoryId);
            if (menuCategory == null)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuCategoryId does not exist.",
                    Data = null
                };
            }

            var exists = await _menuRepository
                .GetAllMenuFiltered(new Menu { MenuName = normalizedName }, null, null)
                .AnyAsync(m => m.MenuName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuName is existed.",
                    Data = null
                };
            }

            var entity = new Menu
            {
                MenuName = normalizedName,
                MenuCategoryId = request.MenuCategoryId,
                BasePrice = request.BasePrice,
                ImgUrl = request.ImgUrl?.Trim(),
                Status = MenuStatus.AVAILABLE.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            var affected = await _menuRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<MenuResponse>();
                return new ApiResponse<MenuResponse>
                {
                    Success = true,
                    Message = "Menu created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<MenuResponse>
            {
                Success = false,
                Message = "Failed to create menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<MenuResponse>> UpdateAsync(int id, MenuUpdateRequest request)
        {
            var entity = await _menuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            var normalizedName = request.MenuName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuName is required.",
                    Data = null
                };
            }

            var menuCategory = await _menuCategoryRepository.GetByIdAsync(request.MenuCategoryId);
            if (menuCategory == null)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuCategoryId does not exist.",
                    Data = null
                };
            }

            var exists = await _menuRepository
                .GetAllMenuFiltered(new Menu { MenuName = normalizedName }, null, null)
                .AnyAsync(m => m.MenuId != id && m.MenuName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuName is existed.",
                    Data = null
                };
            }

            entity.MenuName = normalizedName;
            entity.MenuCategoryId = request.MenuCategoryId;
            entity.BasePrice = request.BasePrice;
            entity.ImgUrl = request.ImgUrl?.Trim();
            entity.Status = request.Status.ToString();

            var affected = await _menuRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<MenuResponse>();
                return new ApiResponse<MenuResponse>
                {
                    Success = true,
                    Message = "Menu updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<MenuResponse>
            {
                Success = false,
                Message = "Failed to update menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _menuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = false
                };
            }

            if (await _menuRepository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete menu because it is being used in related data.",
                    Data = false
                };
            }

            var removed = await _menuRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Menu deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete menu.",
                Data = false
            };
        }
    }
}