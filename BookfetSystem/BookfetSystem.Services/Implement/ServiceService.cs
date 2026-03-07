using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Linq;

namespace BookfetSystem.Services.Services
{
    public class ServiceService : IServiceService
    {
        private readonly ServiceRepository _repository;

        public ServiceService(ServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ServiceResponse>> GetAll(ServiceFilterRequest filter)
        {
            var entityFilter = new BookfetSystem.Repositories.Entities.Service
            {
                ServiceId = filter.ServiceId ?? 0,
                ServiceName = filter.ServiceName,
                Status = filter.Status
            };

            var data = _repository.GetAllServiceFiltered(entityFilter).ToList();

            return data.Select(x => new ServiceResponse
            {
                ServiceId = x.ServiceId,
                ServiceName = x.ServiceName,
                Description = x.Description,
                BasePrice = x.BasePrice,
                Status = x.Status,
                Img = x.Img,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<ServiceResponse?> GetById(int id)
        {
            var entity = await _repository.GetByIdWithRelationAsync(id);

            if (entity == null) return null;

            return new ServiceResponse
            {
                ServiceId = entity.ServiceId,
                ServiceName = entity.ServiceName,
                Description = entity.Description,
                BasePrice = entity.BasePrice,
                Status = entity.Status,
                Img = entity.Img,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<bool> Create(ServiceCreateRequest request)
        {
            var entity = new BookfetSystem.Repositories.Entities.Service
            {
                ServiceName = request.ServiceName,
                Description = request.Description,
                BasePrice = request.BasePrice,
                Status = request.Status,
                Img = request.Img,
                CreatedAt = DateTime.Now
            };

            await _repository.CreateAsync(entity);
            return true;
        }

        public async Task<bool> Update(ServiceUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.ServiceId);

            if (entity == null) return false;

            entity.ServiceName = request.ServiceName;
            entity.Description = request.Description;
            entity.BasePrice = request.BasePrice;
            entity.Status = request.Status;
            entity.Img = request.Img;

            await _repository.UpdateAsync(entity);

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null) return false;

            await _repository.RemoveAsync(entity);

            return true;
        }
    }
}