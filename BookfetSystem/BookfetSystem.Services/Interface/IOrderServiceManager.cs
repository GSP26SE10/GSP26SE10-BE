using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderServiceManager
    {
        Task<PagedResponse<OrderServiceResponse>> GetAllOrderServiceFilteredAsync(OrderServiceFilterRequest request, int page, int pageSize);
        Task<ApiResponse<OrderServiceResponse>> CreateAsync(OrderServiceCreateRequest request);
        Task<ApiResponse<OrderServiceResponse>> UpdateAsync(int id, OrderServiceUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
