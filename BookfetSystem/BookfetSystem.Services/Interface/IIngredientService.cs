using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IIngredientService
    {
        Task<PagedResponse<IngredientResponse>> GetAllIngredientFilteredAsync(IngredientFilterRequest request, int page, int pageSize);
        Task<ApiResponse<IngredientResponse>> CreateAsync(IngredientCreateRequest request);
        Task<ApiResponse<IngredientResponse>> UpdateAsync(int id, IngredientUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
