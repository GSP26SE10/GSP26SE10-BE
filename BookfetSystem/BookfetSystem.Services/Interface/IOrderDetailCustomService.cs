using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interfaces
{
    public interface IOrderDetailCustomService
    {
        Task<List<OrderDetailCustomResponse>> GetAll(OrderDetailCustomFilterRequest filter);

        Task<OrderDetailCustomResponse?> GetById(int id);

        Task<bool> Create(OrderDetailCustomCreateRequest request);

        Task<bool> Update(OrderDetailCustomUpdateRequest request);

        Task<bool> Delete(int id);
    }
}