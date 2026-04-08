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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class MenuService : IMenuService
    {
        private const int MaxMenuImagesPerRequest = 3;
        private readonly MenuRepository _menuRepository;
        private readonly MenuCategoryRepository _menuCategoryRepository;
        private readonly PartyCategoryRepository _partyCategoryRepository;
        private readonly PartyCategoryMenuRepository _partyCategoryMenuRepository;
        private readonly IImageStorageService _imageStorageService;

        public MenuService(
            MenuRepository menuRepository,
            MenuCategoryRepository menuCategoryRepository,
            PartyCategoryRepository partyCategoryRepository,
            PartyCategoryMenuRepository partyCategoryMenuRepository,
            IImageStorageService imageStorageService)
        {
            _menuRepository = menuRepository;
            _menuCategoryRepository = menuCategoryRepository;
            _partyCategoryRepository = partyCategoryRepository;
            _partyCategoryMenuRepository = partyCategoryMenuRepository;
            _imageStorageService = imageStorageService;
        }

        public async Task<PagedResponse<MenuResponse>> GetAllMenuFilteredAsync(MenuFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Menu>();
            var query = _menuRepository.GetAllMenuFiltered(entityFilter, request.MinBasePrice, request.MaxBasePrice);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ProjectToType<MenuResponse>()
                .ToListAsync();

            return new PagedResponse<MenuResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
                // Tuyệt đối không viết TotalPages ở đây nữa vì class đã tự tính rồi
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

            if (request.MenuCategoryId <= 0)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuCategoryId is required.",
                    Data = null
                };
            }

            if (request.BasePrice.HasValue && request.BasePrice.Value < 0)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "BasePrice must be greater than or equal to 0.",
                    Data = null
                };
            }

            var partyCategoryIds = NormalizeIdList(request.PartyCategoryIds);
            if (partyCategoryIds.Count == 0)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "At least one PartyCategoryId is required.",
                    Data = null
                };
            }

            var menuCategory = await _menuCategoryRepository.GetByIdAsync(request.MenuCategoryId);
            if (menuCategory == null)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = $"MenuCategoryId does not exist: {request.MenuCategoryId}.",
                    Data = null
                };
            }

            var existingPartyCategoryIds = await _partyCategoryRepository
                .GetAllPartyCategoryFiltered(new PartyCategory())
                .Where(pc => partyCategoryIds.Contains(pc.PartyCategoryId))
                .Select(pc => pc.PartyCategoryId)
                .ToListAsync();
            var missingPartyCategoryIds = partyCategoryIds.Except(existingPartyCategoryIds).ToList();
            if (missingPartyCategoryIds.Count > 0)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = $"PartyCategoryIds do not exist: {string.Join(", ", missingPartyCategoryIds)}.",
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
                Status = MenuStatus.AVAILABLE.ToString(),
                CreatedAt = DateTime.UtcNow,
                PartyCategoryMenus = partyCategoryIds
                    .Select(partyCategoryId => new PartyCategoryMenu
                    {
                        PartyCategoryId = partyCategoryId
                    })
                    .ToList()
            };

            try
            {
                var files = NormalizeMenuImageFiles(request.ImgFiles);
                if (files.Count > MaxMenuImagesPerRequest)
                {
                    return new ApiResponse<MenuResponse>
                    {
                        Success = false,
                        Message = $"You can upload up to {MaxMenuImagesPerRequest} images at once.",
                        Data = null
                    };
                }

                if (files.Count > 0)
                {
                    var uploadedUrls = new List<string>(files.Count);
                    foreach (var file in files)
                    {
                        var uploadedUrl = await _imageStorageService.UploadImageAsync(file, CloudinaryFolder.Menu);
                        uploadedUrls.Add(uploadedUrl);
                    }

                    entity.ImgUrl = JsonSerializer.Serialize(uploadedUrls);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = $"Failed to upload menu image: {ex.Message}",
                    Data = null
                };
            }

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

            if (request.MenuCategoryId <= 0)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "MenuCategoryId is required.",
                    Data = null
                };
            }

            var partyCategoryIds = NormalizeIdList(request.PartyCategoryIds);
            if (partyCategoryIds.Count == 0)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = "At least one PartyCategoryId is required.",
                    Data = null
                };
            }

            var menuCategory = await _menuCategoryRepository.GetByIdAsync(request.MenuCategoryId);
            if (menuCategory == null)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = $"MenuCategoryId does not exist: {request.MenuCategoryId}.",
                    Data = null
                };
            }

            var existingPartyCategoryIds = await _partyCategoryRepository
                .GetAllPartyCategoryFiltered(new PartyCategory())
                .Where(pc => partyCategoryIds.Contains(pc.PartyCategoryId))
                .Select(pc => pc.PartyCategoryId)
                .ToListAsync();
            var missingPartyCategoryIds = partyCategoryIds.Except(existingPartyCategoryIds).ToList();
            if (missingPartyCategoryIds.Count > 0)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = $"PartyCategoryIds do not exist: {string.Join(", ", missingPartyCategoryIds)}.",
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
            entity.Status = request.Status.ToString();

            try
            {
                var files = NormalizeMenuImageFiles(request.ImgFiles);
                if (files.Count > MaxMenuImagesPerRequest)
                {
                    return new ApiResponse<MenuResponse>
                    {
                        Success = false,
                        Message = $"You can upload up to {MaxMenuImagesPerRequest} images at once.",
                        Data = null
                    };
                }

                if (files.Count > 0)
                {
                    var uploadedUrls = new List<string>(files.Count);
                    foreach (var file in files)
                    {
                        var uploadedUrl = await _imageStorageService.UploadImageAsync(file, CloudinaryFolder.Menu, id);
                        uploadedUrls.Add(uploadedUrl);
                    }

                    entity.ImgUrl = JsonSerializer.Serialize(uploadedUrls);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<MenuResponse>
                {
                    Success = false,
                    Message = $"Failed to upload menu image: {ex.Message}",
                    Data = null
                };
            }

            var affected = await _menuRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var existingPartyCategoryMenuRows = await _partyCategoryMenuRepository
                    .GetAllPartyCategoryMenuFiltered(new PartyCategoryMenu { MenuId = id })
                    .ToListAsync();

                var existingPartyCategoryIdSet = existingPartyCategoryMenuRows
                    .Where(x => x.PartyCategoryId.HasValue)
                    .Select(x => x.PartyCategoryId!.Value)
                    .ToHashSet();

                var requestedPartyCategoryIdSet = partyCategoryIds.ToHashSet();

                var rowsToRemove = existingPartyCategoryMenuRows
                    .Where(x => x.PartyCategoryId.HasValue && !requestedPartyCategoryIdSet.Contains(x.PartyCategoryId.Value))
                    .ToList();

                foreach (var row in rowsToRemove)
                {
                    await _partyCategoryMenuRepository.RemoveAsync(row);
                }

                var partyCategoryIdsToAdd = requestedPartyCategoryIdSet
                    .Where(x => !existingPartyCategoryIdSet.Contains(x))
                    .ToList();

                foreach (var partyCategoryId in partyCategoryIdsToAdd)
                {
                    var partyCategoryMenu = new PartyCategoryMenu
                    {
                        PartyCategoryId = partyCategoryId,
                        MenuId = id
                    };
                    await _partyCategoryMenuRepository.CreateAsync(partyCategoryMenu);
                }

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

        private static List<Microsoft.AspNetCore.Http.IFormFile> NormalizeMenuImageFiles(
            List<Microsoft.AspNetCore.Http.IFormFile>? fileList)
        {
            var files = new List<Microsoft.AspNetCore.Http.IFormFile>();

            if (fileList != null && fileList.Count > 0)
            {
                files.AddRange(fileList.Where(f => f != null && f.Length > 0));
            }

            return files;
        }

        private static List<int> NormalizeIdList(List<int>? ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<int>();
            }

            return ids
                .Where(x => x > 0)
                .Distinct()
                .ToList();
        }
    }
}