using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderDetailService
    {
        Task<PagedResponse<OrderDetailResponse>> GetAllFilteredAsync(OrderDetailFilterRequest filter, int page, int pageSize);

        Task<OrderDetailResponse?> GetById(int id);

        Task<ApiResponse<OrderDetailResponse>> Create(OrderDetailCreateRequest request);

        Task<ApiResponse<OrderDetailResponse>> Update(int id, OrderDetailUpdateRequest request);

        Task<ApiResponse<bool>> Delete(int id);
    }
}