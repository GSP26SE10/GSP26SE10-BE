using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IPostBlockService
    {
        Task<PagedResponse<PostBlockResponse>> GetAllPostBlockFilteredAsync(PostBlockFilterRequest request, int page, int pageSize);
        Task<ApiResponse<PostBlockResponse>> CreateAsync(PostBlockCreateRequest request);
        Task<ApiResponse<PostBlockResponse>> UpdateAsync(int id, PostBlockUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
