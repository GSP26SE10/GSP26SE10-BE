using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class ServiceExtraChargeCatalogService : IServiceExtraChargeCatalogService
    {
        private readonly ServiceExtraChargeCatalogRepository _repository;
        private readonly ServiceRepository _serviceRepository;
        private readonly ExtraChargeCatalogRepository _extraChargeCatalogRepository;

        public ServiceExtraChargeCatalogService(
            ServiceExtraChargeCatalogRepository repository,
            ServiceRepository serviceRepository,
            ExtraChargeCatalogRepository extraChargeCatalogRepository)
        {
            _repository = repository;
            _serviceRepository = serviceRepository;
            _extraChargeCatalogRepository = extraChargeCatalogRepository;
        }

        public async Task<PagedResponse<ServiceExtraChargeCatalogResponse>> GetAllFilteredAsync(ServiceExtraChargeCatalogFilterRequest filter, int page, int pageSize)
        {
            var entityFilter = new ServiceExtraChargeCatalog
            {
                ServiceExtraChargeCatalogId = filter.ServiceExtraChargeCatalogId,
                ServiceId = filter.ServiceId ?? 0,
                ExtraChargeCatalogId = filter.ExtraChargeCatalogId ?? 0
            };

            var query = _repository.GetAllFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            return new PagedResponse<ServiceExtraChargeCatalogResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<ServiceExtraChargeCatalogResponse>> CreateAsync(ServiceExtraChargeCatalogCreateRequest request)
        {
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = null
                };
            }

            var extraCatalog = await _extraChargeCatalogRepository.GetByIdAsync(request.ExtraChargeCatalogId);
            if (extraCatalog == null)
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Extra charge catalog not found.",
                    Data = null
                };
            }

            if (await _repository.ExistsAsync(request.ServiceId, request.ExtraChargeCatalogId))
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "This service-extra charge catalog mapping already exists.",
                    Data = null
                };
            }

            var entity = new ServiceExtraChargeCatalog
            {
                ServiceId = request.ServiceId,
                ExtraChargeCatalogId = request.ExtraChargeCatalogId,
                CreatedAt = DateTime.UtcNow
            };

            var affected = await _repository.CreateAsync(entity);
            if (affected <= 0)
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Failed to create service-extra charge catalog mapping.",
                    Data = null
                };
            }

            var response = await _repository
                .GetAllFiltered(new ServiceExtraChargeCatalog { ServiceExtraChargeCatalogId = entity.ServiceExtraChargeCatalogId })
                .Select(x => ToResponse(x))
                .FirstOrDefaultAsync();

            return new ApiResponse<ServiceExtraChargeCatalogResponse>
            {
                Success = true,
                Message = "Service-extra charge catalog mapping created successfully.",
                Data = response
            };
        }

        public async Task<ApiResponse<ServiceExtraChargeCatalogResponse>> UpdateAsync(int id, ServiceExtraChargeCatalogUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Service-extra charge catalog mapping not found.",
                    Data = null
                };
            }

            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = null
                };
            }

            var extraCatalog = await _extraChargeCatalogRepository.GetByIdAsync(request.ExtraChargeCatalogId);
            if (extraCatalog == null)
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Extra charge catalog not found.",
                    Data = null
                };
            }

            if (await _repository.ExistsAsync(request.ServiceId, request.ExtraChargeCatalogId, id))
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "This service-extra charge catalog mapping already exists.",
                    Data = null
                };
            }

            entity.ServiceId = request.ServiceId;
            entity.ExtraChargeCatalogId = request.ExtraChargeCatalogId;

            var affected = await _repository.UpdateAsync(entity);
            if (affected <= 0)
            {
                return new ApiResponse<ServiceExtraChargeCatalogResponse>
                {
                    Success = false,
                    Message = "Failed to update service-extra charge catalog mapping.",
                    Data = null
                };
            }

            var response = await _repository
                .GetAllFiltered(new ServiceExtraChargeCatalog { ServiceExtraChargeCatalogId = id })
                .Select(x => ToResponse(x))
                .FirstOrDefaultAsync();

            return new ApiResponse<ServiceExtraChargeCatalogResponse>
            {
                Success = true,
                Message = "Service-extra charge catalog mapping updated successfully.",
                Data = response
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
                    Message = "Service-extra charge catalog mapping not found.",
                    Data = false
                };
            }

            var removed = await _repository.RemoveAsync(entity);
            if (!removed)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to delete service-extra charge catalog mapping.",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Service-extra charge catalog mapping deleted successfully.",
                Data = true
            };
        }

        private static ServiceExtraChargeCatalogResponse ToResponse(ServiceExtraChargeCatalog entity)
        {
            return new ServiceExtraChargeCatalogResponse
            {
                ServiceExtraChargeCatalogId = entity.ServiceExtraChargeCatalogId,
                ServiceId = entity.ServiceId,
                ExtraChargeCatalogId = entity.ExtraChargeCatalogId,
                ServiceName = entity.Service?.ServiceName,
                ExtraChargeCatalogTitle = entity.ExtraChargeCatalog?.Title,
                ExtraChargeType = entity.ExtraChargeCatalog?.ChargeType,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
