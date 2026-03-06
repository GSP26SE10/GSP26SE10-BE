using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly OrderDetailRepository _repository;

        public OrderDetailService(OrderDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Create(OrderDetailRequest request)
        {
            var entity = new OrderDetail
            {
                OrderId = request.OrderId,
                Address = request.Address,
                NumberOfGuests = request.NumberOfGuests,
                Status = request.Status,
                TotalPrice = request.TotalPrice,
                Type = request.Type,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                StaffGroupId = request.StaffGroupId,
                PartyCategoryId = request.PartyCategoryId,
                MenuId = request.MenuId
            };

            await _repository.CreateAsync(entity);

            return true;
        }

        public async Task<bool> Update(OrderDetailRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.OrderDetailId.Value);

            if (entity == null)
                return false;

            entity.Address = request.Address;
            entity.NumberOfGuests = request.NumberOfGuests;
            entity.Status = request.Status;
            entity.TotalPrice = request.TotalPrice;
            entity.Type = request.Type;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.StaffGroupId = request.StaffGroupId;
            entity.PartyCategoryId = request.PartyCategoryId;
            entity.MenuId = request.MenuId;

            await _repository.UpdateAsync(entity);

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                return false;

            await _repository.RemoveAsync(entity);

            return true;
        }

        public async Task<List<OrderDetailResponse>> GetAll(OrderDetailRequest filter)
        {
            var entityFilter = new OrderDetail
            {
                OrderDetailId = filter.OrderDetailId ?? 0,
                OrderId = filter.OrderId,
                Status = filter.Status,
                MenuId = filter.MenuId,
                PartyCategoryId = filter.PartyCategoryId
            };

            var list = _repository.GetAllOrderDetailFiltered(entityFilter).ToList();

            return list.Select(x => new OrderDetailResponse
            {
                OrderDetailId = x.OrderDetailId,
                OrderId = x.OrderId,
                Address = x.Address,
                NumberOfGuests = x.NumberOfGuests,
                Status = x.Status,
                TotalPrice = x.TotalPrice,
                Type = x.Type,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                StaffGroupId = x.StaffGroupId,
                PartyCategoryId = x.PartyCategoryId,
                MenuId = x.MenuId,
                MenuName = x.Menu?.MenuName,
                PartyCategoryName = x.PartyCategory?.PartyCategoryName
            }).ToList();
        }

        public async Task<OrderDetailResponse?> GetById(int id)
        {
            var x = await _repository.GetByIdWithRelationAsync(id);

            if (x == null)
                return null;

            return new OrderDetailResponse
            {
                OrderDetailId = x.OrderDetailId,
                OrderId = x.OrderId,
                Address = x.Address,
                NumberOfGuests = x.NumberOfGuests,
                Status = x.Status,
                TotalPrice = x.TotalPrice,
                Type = x.Type,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                StaffGroupId = x.StaffGroupId,
                PartyCategoryId = x.PartyCategoryId,
                MenuId = x.MenuId,
                MenuName = x.Menu?.MenuName,
                PartyCategoryName = x.PartyCategory?.PartyCategoryName
            };
        }
    }
}