using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Services.Services
{
    public class ServiceService : IServiceService
    {
        private readonly ServiceRepository _repository;

        public ServiceService(ServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<ServiceResponse>> GetAllFilteredAsync(ServiceFilterRequest filter, int page, int pageSize)
        {
            var entityFilter = filter.Adapt<Repositories.Entities.Service>();
            var query = _repository.GetAllServiceFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var data = await query
                .ProjectToType<ServiceResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<ServiceResponse>
            {
                Items = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ServiceResponse?> GetById(int id)
        {
            var entity = await _repository.GetByIdWithRelationAsync(id);

            if (entity == null) return null;

            return entity.Adapt<ServiceResponse>();
        }

        public async Task<ApiResponse<ServiceResponse>> Create(ServiceCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ServiceName))
            {
                return new ApiResponse<ServiceResponse>
                {
                    Success = false,
                    Message = "ServiceName is required.",
                    Data = null
                };
            }

            var entity = new Repositories.Entities.Service 
            {
                ServiceName = request.ServiceName.Trim(),
                Description = request.Description,
                BasePrice = request.BasePrice,
                Status = (request.Status ?? ServiceStatus.AVAILABLE).ToString(),
                Img = request.Img,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _repository.CreateAsync(entity);
                var created = await GetById(entity.ServiceId);

                return new ApiResponse<ServiceResponse>
                {
                    Success = true,
                    Message = "Service created successfully.",
                    Data = created
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<ServiceResponse>
                {
                    Success = false,
                    Message = "Create service failed.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<ServiceResponse>> Update(int id, ServiceUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<ServiceResponse>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = null
                };
            }

            if (!string.IsNullOrWhiteSpace(request.ServiceName))
            {
                entity.ServiceName = request.ServiceName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = request.Description;
            }

            if (request.BasePrice.HasValue)
            {
                entity.BasePrice = request.BasePrice;
            }

            if (request.Status.HasValue)
            {
                entity.Status = request.Status.Value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(request.Img))
            {
                entity.Img = request.Img;
            }

            if (string.IsNullOrWhiteSpace(entity.Status))
            {
                entity.Status = ServiceStatus.AVAILABLE.ToString();
            }

            try
            {
                await _repository.UpdateAsync(entity);
                var updated = await GetById(entity.ServiceId);

                return new ApiResponse<ServiceResponse>
                {
                    Success = true,
                    Message = "Service updated successfully.",
                    Data = updated
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<ServiceResponse>
                {
                    Success = false,
                    Message = "Update service failed.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Service not found.",
                    Data = false
                };
            }

            var hasRelatedData = await _repository.HasRelatedDataAsync(id);
            if (hasRelatedData)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete service because it is referenced by order service records.",
                    Data = false
                };
            }

            try
            {
                await _repository.RemoveAsync(entity);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Service deleted successfully.",
                    Data = true
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Delete service failed due to related data constraints.",
                    Data = false
                };
            }
        }
    }
}