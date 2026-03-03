using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IBlogCategoryService
    {
        Task<PagedResponse<BlogCategoryResponse>> GetAllBlogCategoryFilteredAsync(BlogCategoryFilterRequest request, int page, int pageSize);
        Task<ApiResponse<BlogCategoryResponse>> CreateAsync(BlogCategoryCreateRequest request);
        Task<ApiResponse<BlogCategoryResponse>> UpdateAsync(int id, BlogCategoryUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
