using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderDetailStaffTaskService
    {
        Task<PagedResponse<OrderDetailStaffTaskResponse>> GetAllOrderDetailStaffTaskFilteredAsync(OrderDetailStaffTaskFilterRequest request, int page, int pageSize);
        Task<ApiResponse<OrderDetailStaffTaskResponse>> CreateAsync(OrderDetailStaffTaskCreateRequest request);
        Task<ApiResponse<OrderDetailStaffTaskResponse>> UpdateAsync(int id, OrderDetailStaffTaskUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
