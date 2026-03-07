using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interfaces
{
    public interface IOrderServiceManager
    {
        Task<List<OrderServiceResponse>> GetAll(OrderServiceFilterRequest filter);

        Task<OrderServiceResponse?> GetById(int id);

        Task<bool> Create(OrderServiceCreateRequest request);

        Task<bool> Update(OrderServiceUpdateRequest request);

        Task<bool> Delete(int id);
    }
}