using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IDishCategoryService
    {
        Task<PagedResponse<DishCategoryResponse>> GetAllDishCategoryFilteredAsync(DishCategoryFilterRequest request, int page, int pageSize);
        Task<ApiResponse<DishCategoryResponse>> CreateAsync(DishCategoryCreateRequest request);
        Task<ApiResponse<DishCategoryResponse>> UpdateAsync(int id, DishCategoryUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
