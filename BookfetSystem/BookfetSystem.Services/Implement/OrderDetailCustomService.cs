using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Linq;

namespace BookfetSystem.Services.Services
{
    public class OrderDetailCustomService : IOrderDetailCustomService
    {
        private readonly OrderDetailCustomRepository _repository;

        public OrderDetailCustomService(OrderDetailCustomRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OrderDetailCustomResponse>> GetAll(OrderDetailCustomFilterRequest filter)
        {
            var entityFilter = new OrderDetailCustom
            {
                OrderDetailCustomId = filter.OrderDetailCustomId ?? 0,
                OrderDetailId = filter.OrderDetailId,
                DishId = filter.DishId,
                Quantity = filter.Quantity
            };

            var data = _repository.GetAllOrderDetailCustomFiltered(entityFilter).ToList();

            return data.Select(x => new OrderDetailCustomResponse
            {
                OrderDetailCustomId = x.OrderDetailCustomId,
                OrderDetailId = x.OrderDetailId,
                DishId = x.DishId,
                Quantity = x.Quantity,
                TotalAmount = x.TotalAmount,
                DishName = x.Dish?.DishName
            }).ToList();
        }

        public async Task<OrderDetailCustomResponse?> GetById(int id)
        {
            var entity = await _repository.GetByIdWithRelationAsync(id);

            if (entity == null) return null;

            return new OrderDetailCustomResponse
            {
                OrderDetailCustomId = entity.OrderDetailCustomId,
                OrderDetailId = entity.OrderDetailId,
                DishId = entity.DishId,
                Quantity = entity.Quantity,
                TotalAmount = entity.TotalAmount,
                DishName = entity.Dish?.DishName
            };
        }

        public async Task<bool> Create(OrderDetailCustomCreateRequest request)
        {
            var entity = new OrderDetailCustom
            {
                OrderDetailId = request.OrderDetailId,
                DishId = request.DishId,
                Quantity = request.Quantity,
                TotalAmount = request.TotalAmount
            };

            await _repository.CreateAsync(entity);
            return true;
        }

        public async Task<bool> Update(OrderDetailCustomUpdateRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.OrderDetailCustomId);

            if (entity == null) return false;

            entity.OrderDetailId = request.OrderDetailId;
            entity.DishId = request.DishId;
            entity.Quantity = request.Quantity;
            entity.TotalAmount = request.TotalAmount;

            await _repository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null) return false;

            await _repository.DeleteAsync(entity);
            return true;
        }
    }
}