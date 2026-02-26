using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IDishService
    {
        Task<PagedResponse<DishResponse>> GetAllDishFilteredAsync(DishFilterRequest request, int page, int pageSize);
        Task<ApiResponse<DishResponse>> CreateAsync(DishCreateRequest request);
        Task<ApiResponse<DishResponse>> UpdateAsync(int id, DishUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
