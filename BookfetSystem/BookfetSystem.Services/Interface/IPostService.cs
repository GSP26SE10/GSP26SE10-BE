using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IPostService
    {
        Task<PagedResponse<PostResponse>> GetAllPostFilteredAsync(PostFilterRequest request, int page, int pageSize);
        Task<ApiResponse<PostResponse>> GetByIdAsync(int id);
        Task<ApiResponse<PostResponse>> CreateAsync(PostCreateRequest request);
        Task<ApiResponse<PostResponse>> UpdateAsync(int id, PostUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
