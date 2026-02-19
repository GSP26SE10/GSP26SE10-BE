using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IStaffGroupMemberService
    {
        Task<PagedResponse<StaffGroupMemberResponse>> GetAllStaffGroupMemberFilteredAsync(StaffGroupMemberFilterRequest request, int page, int pageSize);
        Task<ApiResponse<StaffGroupMemberResponse>> CreateAsync(StaffGroupMemberCreateRequest request);
        Task<ApiResponse<StaffGroupMemberResponse>> UpdateAsync(int id, StaffGroupMemberUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
