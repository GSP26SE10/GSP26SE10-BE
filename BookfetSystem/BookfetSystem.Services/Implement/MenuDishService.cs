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
    public class MenuDishService : IMenuDishService
    {
        private readonly MenuDishRepository _menuDishRepository;
        private readonly MenuRepository _menuRepository;
        private readonly DishRepository _dishRepository;

        public MenuDishService(
            MenuDishRepository menuDishRepository,
            MenuRepository menuRepository,
            DishRepository dishRepository)
        {
            _menuDishRepository = menuDishRepository;
            _menuRepository = menuRepository;
            _dishRepository = dishRepository;
        }

        public async Task<PagedResponse<MenuDishResponse>> GetAllMenuDishFilteredAsync(MenuDishFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<MenuDish>();
            var query = _menuDishRepository.GetAllMenuDishFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(md => new MenuDishResponse
                {
                    MenuDishId = md.MenuDishId,
                    MenuId = md.MenuId,
                    DishId = md.DishId,
                    MenuName = md.Menu != null ? md.Menu.MenuName : null,
                    DishName = md.Dish != null ? md.Dish.DishName : null
                })
                .ToListAsync();

            return new PagedResponse<MenuDishResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<MenuDishResponse>> CreateAsync(MenuDishCreateRequest request)
        {
            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<MenuDishResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            var dish = await _dishRepository.GetByIdAsync(request.DishId);
            if (dish == null)
            {
                return new ApiResponse<MenuDishResponse>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = null
                };
            }

            if (await _menuDishRepository.ExistsAsync(request.MenuId, request.DishId))
            {
                return new ApiResponse<MenuDishResponse>
                {
                    Success = false,
                    Message = "This dish already exists in the selected menu.",
                    Data = null
                };
            }

            var entity = new MenuDish
            {
                MenuId = request.MenuId,
                DishId = request.DishId
            };

            var affected = await _menuDishRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = new MenuDishResponse
                {
                    MenuDishId = entity.MenuDishId,
                    MenuId = entity.MenuId,
                    DishId = entity.DishId,
                    MenuName = menu.MenuName,
                    DishName = dish.DishName
                };

                return new ApiResponse<MenuDishResponse>
                {
                    Success = true,
                    Message = "Menu dish created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<MenuDishResponse>
            {
                Success = false,
                Message = "Failed to create menu dish.",
                Data = null
            };
        }

        public async Task<ApiResponse<MenuDishResponse>> UpdateAsync(int id, MenuDishUpdateRequest request)
        {
            var entity = await _menuDishRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<MenuDishResponse>
                {
                    Success = false,
                    Message = "Menu dish not found.",
                    Data = null
                };
            }

            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<MenuDishResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            var dish = await _dishRepository.GetByIdAsync(request.DishId);
            if (dish == null)
            {
                return new ApiResponse<MenuDishResponse>
                {
                    Success = false,
                    Message = "Dish not found.",
                    Data = null
                };
            }

            if (await _menuDishRepository.ExistsAsync(request.MenuId, request.DishId, id))
            {
                return new ApiResponse<MenuDishResponse>
                {
                    Success = false,
                    Message = "This dish already exists in the selected menu.",
                    Data = null
                };
            }

            entity.MenuId = request.MenuId;
            entity.DishId = request.DishId;

            var affected = await _menuDishRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = new MenuDishResponse
                {
                    MenuDishId = entity.MenuDishId,
                    MenuId = entity.MenuId,
                    DishId = entity.DishId,
                    MenuName = menu.MenuName,
                    DishName = dish.DishName
                };

                return new ApiResponse<MenuDishResponse>
                {
                    Success = true,
                    Message = "Menu dish updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<MenuDishResponse>
            {
                Success = false,
                Message = "Failed to update menu dish.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _menuDishRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Menu dish not found.",
                    Data = false
                };
            }

            var removed = await _menuDishRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Menu dish deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete menu dish.",
                Data = false
            };
        }
    }
}
