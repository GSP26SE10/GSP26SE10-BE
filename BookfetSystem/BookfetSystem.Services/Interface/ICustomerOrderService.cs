using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface ICustomerOrderService
    {
        Task<PagedResponse<OrderResponse>> GetAllFilteredAsync(OrderFilterRequest filter, int page, int pageSize);

        Task<OrderResponse?> GetById(int id);

        Task<ApiResponse<OrderResponse>> Create(OrderCreateRequest request);

        Task<ApiResponse<OrderResponse>> Update(int id, OrderUpdateRequest request);

        Task<ApiResponse<bool>> Delete(int id);
    }
}