using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderDetailService
    {
        Task<List<OrderDetailResponse>> GetAll(OrderDetailRequest filter);

        Task<OrderDetailResponse?> GetById(int id);

        Task<bool> Create(OrderDetailRequest request);

        Task<bool> Update(OrderDetailRequest request);

        Task<bool> Delete(int id);
    }
}