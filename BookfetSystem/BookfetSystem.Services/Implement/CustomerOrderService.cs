using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Request.BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Linq;

namespace BookfetSystem.Services.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly OrderRepository _repository;

        public CustomerOrderService(OrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OrderResponse>> GetAll(OrderFilterRequest filter)
        {
            var entityFilter = new Order
            {
                OrderId = filter.OrderId ?? 0,
                CustomerId = filter.CustomerId,
                Status = filter.Status
            };

            var data = _repository.GetAllOrderFiltered(entityFilter).ToList();

            return data.Select(x => new OrderResponse
            {
                OrderId = x.OrderId,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer?.FullName,
                Status = x.Status,
                TotalPrice = x.TotalPrice,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<OrderResponse?> GetById(int id)
        {
            var entity = await _repository.GetByIdWithRelationAsync(id);

            if (entity == null) return null;

            return new OrderResponse
            {
                OrderId = entity.OrderId,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer?.FullName,
                Status = entity.Status,
                TotalPrice = entity.TotalPrice,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<bool> Create(OrderCreateRequest request)
        {
            var entity = new Order
            {
                CustomerId = request.CustomerId,
                Status = request.Status,
                TotalPrice = request.TotalPrice,
                CreatedAt = DateTime.Now
            };

            await _repository.CreateAsync(entity);
            return true;
        }

        public async Task<bool> Update(OrderUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.OrderId);

            if (entity == null) return false;

            entity.CustomerId = request.CustomerId;
            entity.Status = request.Status;
            entity.TotalPrice = request.TotalPrice;

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