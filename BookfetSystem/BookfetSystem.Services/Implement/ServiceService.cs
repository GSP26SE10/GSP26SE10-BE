using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class ServiceService : IServiceService
    {
        private readonly ServiceRepository _serviceRepository;

        public ServiceService(ServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<object> GetAllServiceFilteredAsync(ServiceFilterRequest filter, int page, int pageSize)
        {
            var serviceFilter = filter.Adapt<Service>();

            var query = _serviceRepository.GetAllServiceFiltered(serviceFilter, null, null);

            var total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Data = data
            };
        }

        public async Task<object> CreateAsync(ServiceCreateRequest request)
        {
            var service = request.Adapt<Service>();

            service.CreatedAt = DateTime.UtcNow;

            await _serviceRepository.CreateAsync(service);
            await _serviceRepository.SaveAsync();

            return new
            {
                Success = true,
                Message = "Service created successfully"
            };
        }

        public async Task<object> UpdateAsync(int id, ServiceUpdateRequest request)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return new
                {
                    Success = false,
                    Message = "Service not found"
                };
            }

            request.Adapt(service);

            _serviceRepository.Update(service);
            await _serviceRepository.SaveAsync();

            return new
            {
                Success = true,
                Message = "Service updated successfully"
            };
        }

        public async Task<object> DeleteAsync(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return new
                {
                    Success = false,
                    Message = "Service not found"
                };
            }

            var hasRelated = await _serviceRepository.HasRelatedDataAsync(id);

            if (hasRelated)
            {
                return new
                {
                    Success = false,
                    Message = "Cannot delete service because it has related data"
                };
            }

            _serviceRepository.Remove(service);
            await _serviceRepository.SaveAsync();

            return new
            {
                Success = true,
                Message = "Service deleted successfully"
            };
        }
    }
}