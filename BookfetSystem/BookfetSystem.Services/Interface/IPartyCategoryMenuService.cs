using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IPartyCategoryMenuService
    {
        Task<PagedResponse<PartyCategoryMenuResponse>> GetAllPartyCategoryMenuFilteredAsync(PartyCategoryMenuFilterRequest request, int page, int pageSize);
        Task<ApiResponse<PartyCategoryMenuResponse>> CreateAsync(PartyCategoryMenuCreateRequest request);
        Task<ApiResponse<PartyCategoryMenuResponse>> UpdateAsync(int id, PartyCategoryMenuUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
