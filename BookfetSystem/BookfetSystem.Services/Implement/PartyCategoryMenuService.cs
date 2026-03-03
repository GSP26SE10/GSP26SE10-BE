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
    public class PartyCategoryMenuService : IPartyCategoryMenuService
    {
        private readonly PartyCategoryMenuRepository _partyCategoryMenuRepository;
        private readonly PartyCategoryRepository _partyCategoryRepository;
        private readonly MenuRepository _menuRepository;

        public PartyCategoryMenuService(
            PartyCategoryMenuRepository partyCategoryMenuRepository,
            PartyCategoryRepository partyCategoryRepository,
            MenuRepository menuRepository)
        {
            _partyCategoryMenuRepository = partyCategoryMenuRepository;
            _partyCategoryRepository = partyCategoryRepository;
            _menuRepository = menuRepository;
        }

        public async Task<PagedResponse<PartyCategoryMenuResponse>> GetAllPartyCategoryMenuFilteredAsync(PartyCategoryMenuFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<PartyCategoryMenu>();
            var query = _partyCategoryMenuRepository.GetAllPartyCategoryMenuFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<PartyCategoryMenuResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<PartyCategoryMenuResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PartyCategoryMenuResponse>> CreateAsync(PartyCategoryMenuCreateRequest request)
        {
            var partyCategory = await _partyCategoryRepository.GetByIdAsync(request.PartyCategoryId);
            if (partyCategory == null)
            {
                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = false,
                    Message = "Party category not found.",
                    Data = null
                };
            }

            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            if (await _partyCategoryMenuRepository.ExistsAsync(request.PartyCategoryId, request.MenuId))
            {
                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = false,
                    Message = "This menu already exists in the selected party category.",
                    Data = null
                };
            }

            var entity = new PartyCategoryMenu
            {
                PartyCategoryId = request.PartyCategoryId,
                MenuId = request.MenuId
            };

            var affected = await _partyCategoryMenuRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = await _partyCategoryMenuRepository
                    .GetAllPartyCategoryMenuFiltered(new PartyCategoryMenu { PartyCategoryMenuId = entity.PartyCategoryMenuId })
                    .ProjectToType<PartyCategoryMenuResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = true,
                    Message = "Party category menu created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PartyCategoryMenuResponse>
            {
                Success = false,
                Message = "Failed to create party category menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<PartyCategoryMenuResponse>> UpdateAsync(int id, PartyCategoryMenuUpdateRequest request)
        {
            var entity = await _partyCategoryMenuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = false,
                    Message = "Party category menu not found.",
                    Data = null
                };
            }

            var partyCategory = await _partyCategoryRepository.GetByIdAsync(request.PartyCategoryId);
            if (partyCategory == null)
            {
                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = false,
                    Message = "Party category not found.",
                    Data = null
                };
            }

            var menu = await _menuRepository.GetByIdAsync(request.MenuId);
            if (menu == null)
            {
                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = false,
                    Message = "Menu not found.",
                    Data = null
                };
            }

            if (await _partyCategoryMenuRepository.ExistsAsync(request.PartyCategoryId, request.MenuId, id))
            {
                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = false,
                    Message = "This menu already exists in the selected party category.",
                    Data = null
                };
            }

            entity.PartyCategoryId = request.PartyCategoryId;
            entity.MenuId = request.MenuId;

            var affected = await _partyCategoryMenuRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = await _partyCategoryMenuRepository
                    .GetAllPartyCategoryMenuFiltered(new PartyCategoryMenu { PartyCategoryMenuId = entity.PartyCategoryMenuId })
                    .ProjectToType<PartyCategoryMenuResponse>()
                    .FirstOrDefaultAsync();

                return new ApiResponse<PartyCategoryMenuResponse>
                {
                    Success = true,
                    Message = "Party category menu updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<PartyCategoryMenuResponse>
            {
                Success = false,
                Message = "Failed to update party category menu.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _partyCategoryMenuRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Party category menu not found.",
                    Data = false
                };
            }

            var removed = await _partyCategoryMenuRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Party category menu deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete party category menu.",
                Data = false
            };
        }
    }
}
