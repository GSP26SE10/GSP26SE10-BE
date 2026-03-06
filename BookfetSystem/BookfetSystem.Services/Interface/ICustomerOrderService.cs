using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Request.BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface ICustomerOrderService
    {
        Task<List<OrderResponse>> GetAll(OrderFilterRequest filter);

        Task<OrderResponse?> GetById(int id);

        Task<bool> Create(OrderCreateRequest request);

        Task<bool> Update(OrderUpdateRequest request);

        Task<bool> Delete(int id);
    }
}