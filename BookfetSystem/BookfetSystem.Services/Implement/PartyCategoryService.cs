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
    public class PartyCategoryService : IPartyCategoryService
    {
        private readonly PartyCategoryRepository _repository;
        private readonly IImageStorageService _imageStorageService;

        public PartyCategoryService(
            PartyCategoryRepository repository,
            IImageStorageService imageStorageService)
        {
            _repository = repository;
            _imageStorageService = imageStorageService;
        }

        public async Task<PagedResponse<PartyCategoryResponse>> GetAllPartyCategoryFilteredAsync(
            PartyCategoryFilterRequest request, int page, int pageSize)
        {
            var filter = request.Adapt<PartyCategory>();

            var query = _repository.GetAllPartyCategoryFiltered(filter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<PartyCategoryResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<PartyCategoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<PartyCategoryResponse>> CreateAsync(PartyCategoryCreateRequest request)
        {
            var normalizedName = request.PartyCategoryName?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "PartyCategoryName is required."
                };
            }

            var exists = await _repository
                .GetAllPartyCategoryFiltered(new PartyCategory { PartyCategoryName = normalizedName })
                .AnyAsync(pc => pc.PartyCategoryName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "PartyCategoryName is existed."
                };
            }

            if (request.NumberOfGuests <= 0)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "NumberOfGuests must be greater than 0."
                };
            }

            if (request.ServiceDurationMinutes.HasValue && request.ServiceDurationMinutes.Value <= 0)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "ServiceDurationMinutes must be greater than 0."
                };
            }

            var entity = new PartyCategory
            {
                PartyCategoryName = normalizedName,
                Description = request.Description?.Trim(),
                NumberOfGuests = request.NumberOfGuests,
                ServiceDurationMinutes = request.ServiceDurationMinutes,
                Status = PartyCategoryStatus.AVAILABLE.ToString(),
                ImageUrl = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            if (request.ImageUrl != null)
            {
                try
                {
                    entity.ImageUrl = await _imageStorageService.UploadImageAsync(
                        request.ImageUrl,
                        CloudinaryFolder.PartyCategory);
                }
                catch (Exception ex)
                {
                    return new ApiResponse<PartyCategoryResponse>
                    {
                        Success = false,
                        Message = $"Failed to upload party category image: {ex.Message}"
                    };
                }
            }

            var affected = await _repository.CreateAsync(entity);

            if (affected > 0)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = true,
                    Message = "Party category created successfully.",
                    Data = entity.Adapt<PartyCategoryResponse>()
                };
            }

            return new ApiResponse<PartyCategoryResponse>
            {
                Success = false,
                Message = "Failed to create party category."
            };
        }

        public async Task<ApiResponse<PartyCategoryResponse>> UpdateAsync(int id, PartyCategoryUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "Party category not found."
                };
            }

            var normalizedName = request.PartyCategoryName?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "PartyCategoryName is required."
                };
            }

            var exists = await _repository
                .GetAllPartyCategoryFiltered(new PartyCategory { PartyCategoryName = normalizedName })
                .AnyAsync(pc => pc.PartyCategoryId != id &&
                                pc.PartyCategoryName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "PartyCategoryName is existed."
                };
            }

            if (request.NumberOfGuests <= 0)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "NumberOfGuests must be greater than 0."
                };
            }

            if (request.ServiceDurationMinutes.HasValue && request.ServiceDurationMinutes.Value <= 0)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = false,
                    Message = "ServiceDurationMinutes must be greater than 0."
                };
            }

            entity.PartyCategoryName = normalizedName;
            entity.Description = request.Description?.Trim();
            entity.NumberOfGuests = request.NumberOfGuests;
            entity.ServiceDurationMinutes = request.ServiceDurationMinutes;

            if (request.Status.HasValue)
            {
                entity.Status = request.Status.Value.ToString();
            }

            if (request.ImageUrl != null)
            {
                try
                {
                    entity.ImageUrl = await _imageStorageService.UploadImageAsync(
                        request.ImageUrl,
                        CloudinaryFolder.PartyCategory,
                        id);
                }
                catch (Exception ex)
                {
                    return new ApiResponse<PartyCategoryResponse>
                    {
                        Success = false,
                        Message = $"Failed to upload party category image: {ex.Message}"
                    };
                }
            }

            var affected = await _repository.UpdateAsync(entity);

            if (affected > 0)
            {
                return new ApiResponse<PartyCategoryResponse>
                {
                    Success = true,
                    Message = "Party category updated successfully.",
                    Data = entity.Adapt<PartyCategoryResponse>()
                };
            }

            return new ApiResponse<PartyCategoryResponse>
            {
                Success = false,
                Message = "Failed to update party category."
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Party category not found.",
                    Data = false
                };
            }

            if (await _repository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete because related data exists.",
                    Data = false
                };
            }

            var removed = await _repository.RemoveAsync(entity);

            return new ApiResponse<bool>
            {
                Success = removed,
                Message = removed
                    ? "Party category deleted successfully."
                    : "Failed to delete party category.",
                Data = removed
            };
        }
    }
}