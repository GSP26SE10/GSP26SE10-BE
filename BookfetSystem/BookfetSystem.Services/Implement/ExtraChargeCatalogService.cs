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
    public class ExtraChargeCatalogService : IExtraChargeCatalogService
    {
        private readonly ExtraChargeCatalogRepository _extraChargeCatalogRepository;

        public ExtraChargeCatalogService(ExtraChargeCatalogRepository extraChargeCatalogRepository)
        {
            _extraChargeCatalogRepository = extraChargeCatalogRepository;
        }

        public async Task<PagedResponse<ExtraChargeCatalogResponse>> GetAllFilteredAsync(ExtraChargeCatalogFilterRequest request, int page, int pageSize)
        {
            var filter = request.Adapt<ExtraChargeCatalog>();
            var query = _extraChargeCatalogRepository.GetAllFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query
                .ProjectToType<ExtraChargeCatalogResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<ExtraChargeCatalogResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ExtraChargeCatalogResponse>> CreateAsync(ExtraChargeCatalogCreateRequest request)
        {
            var normalizedChargeType = request.ChargeType?.Trim().ToUpperInvariant();
            var normalizedTitle = request.Title?.Trim();
            var normalizedUnit = request.Unit?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedChargeType) ||
                string.IsNullOrWhiteSpace(normalizedTitle) ||
                string.IsNullOrWhiteSpace(normalizedUnit))
            {
                return new ApiResponse<ExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "ChargeType, Title and Unit are required.",
                    Data = null
                };
            }

            var duplicated = await _extraChargeCatalogRepository
                .GetAllFiltered(new ExtraChargeCatalog { ChargeType = normalizedChargeType, Title = normalizedTitle })
                .AnyAsync(x => x.ChargeType.ToLower() == normalizedChargeType.ToLower() &&
                               x.Title.ToLower() == normalizedTitle.ToLower());

            if (duplicated)
            {
                return new ApiResponse<ExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Extra charge catalog with the same charge type and title already exists.",
                    Data = null
                };
            }

            var entity = new ExtraChargeCatalog
            {
                ChargeType = normalizedChargeType,
                Title = normalizedTitle,
                Description = request.Description?.Trim(),
                Unit = normalizedUnit,
                UnitPrice = request.UnitPrice,
                Status = ExtraChargeCatalogStatus.Active.ToString().ToUpperInvariant()
            };

            var affected = await _extraChargeCatalogRepository.CreateAsync(entity);
            if (affected > 0)
            {
                return new ApiResponse<ExtraChargeCatalogResponse>
                {
                    Success = true,
                    Message = "Extra charge catalog created successfully.",
                    Data = entity.Adapt<ExtraChargeCatalogResponse>()
                };
            }

            return new ApiResponse<ExtraChargeCatalogResponse>
            {
                Success = false,
                Message = "Failed to create extra charge catalog.",
                Data = null
            };
        }

        public async Task<ApiResponse<ExtraChargeCatalogResponse>> UpdateAsync(int id, ExtraChargeCatalogUpdateRequest request)
        {
            var entity = await _extraChargeCatalogRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<ExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Extra charge catalog not found.",
                    Data = null
                };
            }

            var normalizedChargeType = request.ChargeType?.Trim().ToUpperInvariant();
            var normalizedTitle = request.Title?.Trim();
            var normalizedUnit = request.Unit?.Trim();
            var normalizedStatus = request.Status.ToString().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(normalizedChargeType) ||
                string.IsNullOrWhiteSpace(normalizedTitle) ||
                string.IsNullOrWhiteSpace(normalizedUnit) ||
                string.IsNullOrWhiteSpace(normalizedStatus))
            {
                return new ApiResponse<ExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "ChargeType, Title, Unit and Status are required.",
                    Data = null
                };
            }

            var duplicated = await _extraChargeCatalogRepository
                .GetAllFiltered(new ExtraChargeCatalog { ChargeType = normalizedChargeType, Title = normalizedTitle })
                .AnyAsync(x => x.ExtraChargeCatalogId != id &&
                               x.ChargeType.ToLower() == normalizedChargeType.ToLower() &&
                               x.Title.ToLower() == normalizedTitle.ToLower());

            if (duplicated)
            {
                return new ApiResponse<ExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Extra charge catalog with the same charge type and title already exists.",
                    Data = null
                };
            }

            entity.ChargeType = normalizedChargeType;
            entity.Title = normalizedTitle;
            entity.Description = request.Description?.Trim();
            entity.Unit = normalizedUnit;
            entity.UnitPrice = request.UnitPrice;
            entity.Status = normalizedStatus;

            var affected = await _extraChargeCatalogRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                return new ApiResponse<ExtraChargeCatalogResponse>
                {
                    Success = true,
                    Message = "Extra charge catalog updated successfully.",
                    Data = entity.Adapt<ExtraChargeCatalogResponse>()
                };
            }

            return new ApiResponse<ExtraChargeCatalogResponse>
            {
                Success = false,
                Message = "Failed to update extra charge catalog.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _extraChargeCatalogRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Extra charge catalog not found.",
                    Data = false
                };
            }

            if (await _extraChargeCatalogRepository.HasRelatedDataAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete extra charge catalog because it is being used by order detail extra charges.",
                    Data = false
                };
            }

            var removed = await _extraChargeCatalogRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Extra charge catalog deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete extra charge catalog.",
                Data = false
            };
        }
    }
}
