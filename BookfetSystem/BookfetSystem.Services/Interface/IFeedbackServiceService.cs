using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IFeedbackServiceService
    {
        Task<PagedResponse<FeedbackServiceResponse>> GetAllFeedbackServiceFilteredAsync(FeedbackServiceFilterRequest request, int page, int pageSize);
        Task<ApiResponse<FeedbackServiceResponse>> CreateAsync(FeedbackServiceCreateRequest request);
        Task<ApiResponse<FeedbackServiceResponse>> UpdateAsync(int id, FeedbackServiceUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
