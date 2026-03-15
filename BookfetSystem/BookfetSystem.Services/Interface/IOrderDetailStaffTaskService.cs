using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderDetailStaffTaskService
    {
        Task<PagedResponse<OrderDetailStaffTaskResponse>> GetAllOrderDetailStaffTaskFilteredAsync(OrderDetailStaffTaskFilterRequest request, int page, int pageSize);
        Task<PagedResponse<StaffMyTaskResponse>> GetMyTasksAsync(int staffId, int page, int pageSize);
        Task<ApiResponse<OrderDetailStaffTaskResponse>> CreateAsync(OrderDetailStaffTaskCreateRequest request, int leaderId);
        Task<ApiResponse<OrderDetailStaffTaskResponse>> UpdateMyTaskStatusAsync(int taskId, int staffId, StaffUpdateTaskStatusRequest request);
        Task<ApiResponse<OrderDetailStaffTaskResponse>> UpdateAsync(int id, OrderDetailStaffTaskUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
