using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderDetailCustomService
    {
        Task<PagedResponse<OrderDetailCustomResponse>> GetAllOrderDetailCustomFilteredAsync(OrderDetailCustomFilterRequest request, int page, int pageSize);
        Task<ApiResponse<OrderDetailCustomResponse>> CreateAsync(OrderDetailCustomCreateRequest request);
        Task<ApiResponse<OrderDetailCustomResponse>> UpdateAsync(int id, OrderDetailCustomUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
