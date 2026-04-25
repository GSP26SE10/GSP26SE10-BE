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
    public class GuestDiscountTierService : IGuestDiscountTierService
    {
        private readonly GuestDiscountTierRepository _guestDiscountTierRepository;

        public GuestDiscountTierService(GuestDiscountTierRepository guestDiscountTierRepository)
        {
            _guestDiscountTierRepository = guestDiscountTierRepository;
        }

        public async Task<PagedResponse<GuestDiscountTierResponse>> GetAllFilteredAsync(GuestDiscountTierFilterRequest filter, int page, int pageSize)
        {
            var entityFilter = filter.Adapt<GuestDiscountTier>();
            var query = _guestDiscountTierRepository.GetAllFiltered(entityFilter);

            var totalCount = await query.CountAsync();
            var items = await query
                .ProjectToType<GuestDiscountTierResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<GuestDiscountTierResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<GuestDiscountTierResponse>> CreateAsync(GuestDiscountTierCreateRequest request)
        {
            if (request.MinGuestCount <= 0)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = false,
                    Message = "MinGuestCount must be greater than 0.",
                    Data = null
                };
            }

            if (request.DiscountPercent < 0 || request.DiscountPercent > 100)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = false,
                    Message = "DiscountPercent must be between 0 and 100.",
                    Data = null
                };
            }

            var duplicated = await _guestDiscountTierRepository
                .GetAllFiltered(new GuestDiscountTier { MinGuestCount = request.MinGuestCount })
                .AnyAsync(x => x.MinGuestCount == request.MinGuestCount);

            if (duplicated)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = false,
                    Message = "MinGuestCount already exists.",
                    Data = null
                };
            }

            var entity = new GuestDiscountTier
            {
                MinGuestCount = request.MinGuestCount,
                DiscountPercent = request.DiscountPercent,
                Note = request.Note?.Trim(),
                Status = GuestDiscountTierStatus.Active.ToString().ToUpperInvariant(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var affected = await _guestDiscountTierRepository.CreateAsync(entity);
            if (affected > 0)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = true,
                    Message = "Guest discount tier created successfully.",
                    Data = entity.Adapt<GuestDiscountTierResponse>()
                };
            }

            return new ApiResponse<GuestDiscountTierResponse>
            {
                Success = false,
                Message = "Failed to create guest discount tier.",
                Data = null
            };
        }

        public async Task<ApiResponse<GuestDiscountTierResponse>> UpdateAsync(int id, GuestDiscountTierUpdateRequest request)
        {
            var entity = await _guestDiscountTierRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = false,
                    Message = "Guest discount tier not found.",
                    Data = null
                };
            }

            if (request.MinGuestCount <= 0)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = false,
                    Message = "MinGuestCount must be greater than 0.",
                    Data = null
                };
            }

            if (request.DiscountPercent < 0 || request.DiscountPercent > 100)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = false,
                    Message = "DiscountPercent must be between 0 and 100.",
                    Data = null
                };
            }

            var duplicated = await _guestDiscountTierRepository
                .GetAllFiltered(new GuestDiscountTier { MinGuestCount = request.MinGuestCount })
                .AnyAsync(x => x.GuestDiscountTierId != id && x.MinGuestCount == request.MinGuestCount);

            if (duplicated)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = false,
                    Message = "MinGuestCount already exists.",
                    Data = null
                };
            }

            entity.MinGuestCount = request.MinGuestCount;
            entity.DiscountPercent = request.DiscountPercent;
            entity.Note = request.Note?.Trim();
            entity.Status = request.Status.ToString().ToUpperInvariant();
            entity.UpdatedAt = DateTime.UtcNow;

            var affected = await _guestDiscountTierRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                return new ApiResponse<GuestDiscountTierResponse>
                {
                    Success = true,
                    Message = "Guest discount tier updated successfully.",
                    Data = entity.Adapt<GuestDiscountTierResponse>()
                };
            }

            return new ApiResponse<GuestDiscountTierResponse>
            {
                Success = false,
                Message = "Failed to update guest discount tier.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _guestDiscountTierRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Guest discount tier not found.",
                    Data = false
                };
            }

            var removed = await _guestDiscountTierRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Guest discount tier deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete guest discount tier.",
                Data = false
            };
        }
    }
}
