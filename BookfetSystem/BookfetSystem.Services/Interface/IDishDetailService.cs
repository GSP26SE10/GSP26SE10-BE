using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IDishDetailService
    {
        Task<PagedResponse<DishDetailResponse>> GetAllDishDetailFilteredAsync(DishDetailFilterRequest request, int page, int pageSize);
        Task<ApiResponse<DishDetailResponse>> CreateAsync(DishDetailCreateRequest request);
        Task<ApiResponse<DishDetailResponse>> UpdateAsync(int id, DishDetailUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
