using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IStaffGroupService
    {
        Task<PagedResponse<StaffGroupResponse>> GetAllStaffGroupFilteredAsync(StaffGroupFilterRequest request, int page, int pageSize);
        Task<ApiResponse<StaffGroupResponse>> CreateAsync(StaffGroupCreateRequest request);
        Task<ApiResponse<StaffGroupResponse>> UpdateAsync(int id, StaffGroupUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<StaffGroupAssignmentOverviewResponse?> GetAssignmentOverviewByLeaderAsync(int leaderId);
    }
}

