using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IFeedbackMenuService
    {
        Task<PagedResponse<FeedbackMenuResponse>> GetAllFeedbackMenuFilteredAsync(FeedbackMenuFilterRequest request, int page, int pageSize);
        Task<ApiResponse<FeedbackMenuResponse>> CreateAsync(FeedbackMenuCreateRequest request);
        Task<ApiResponse<FeedbackMenuResponse>> UpdateAsync(int id, FeedbackMenuUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task ProcessAiSummaryAsync(int menuId);
    }
}
