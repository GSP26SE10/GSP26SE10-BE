using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IMenuCategoryService
    {
        Task<PagedResponse<MenuCategoryResponse>> GetAllMenuCategoryFilteredAsync(MenuCategoryFilterRequest request, int page, int pageSize);
        Task<ApiResponse<MenuCategoryResponse>> CreateAsync(MenuCategoryCreateRequest request);
        Task<ApiResponse<MenuCategoryResponse>> UpdateAsync(int id, MenuCategoryUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
