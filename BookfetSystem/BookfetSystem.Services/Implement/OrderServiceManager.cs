using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Linq;

namespace BookfetSystem.Services.Services
{
    public class OrderServiceManager : IOrderServiceManager
    {
        private readonly OrderServiceRepository _repository;

        public OrderServiceManager(OrderServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OrderServiceResponse>> GetAll(OrderServiceFilterRequest filter)
        {
            var entityFilter = new OrderService
            {
                OrderServiceId = filter.OrderServiceId ?? 0,
                OrderDetailId = filter.OrderDetailId,
                ServiceId = filter.ServiceId
            };

            var data = _repository.GetAllOrderServiceFiltered(entityFilter).ToList();

            return data.Select(x => new OrderServiceResponse
            {
                OrderServiceId = x.OrderServiceId,
                OrderDetailId = x.OrderDetailId,
                ServiceId = x.ServiceId,
                ServiceName = x.Service?.ServiceName,
                Quantity = x.Quantity,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<OrderServiceResponse?> GetById(int id)
        {
            var entity = await _repository.GetByIdWithRelationAsync(id);

            if (entity == null) return null;

            return new OrderServiceResponse
            {
                OrderServiceId = entity.OrderServiceId,
                OrderDetailId = entity.OrderDetailId,
                ServiceId = entity.ServiceId,
                ServiceName = entity.Service?.ServiceName,
                Quantity = entity.Quantity,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<bool> Create(OrderServiceCreateRequest request)
        {
            var entity = new OrderService
            {
                OrderDetailId = request.OrderDetailId,
                ServiceId = request.ServiceId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.Now
            };

            await _repository.CreateAsync(entity);
            return true;
        }

        public async Task<bool> Update(OrderServiceUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.OrderServiceId);

            if (entity == null) return false;

            entity.OrderDetailId = request.OrderDetailId;
            entity.ServiceId = request.ServiceId;
            entity.Quantity = request.Quantity;

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