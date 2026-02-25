using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IMenuDishService
    {
        Task<PagedResponse<MenuDishResponse>> GetAllMenuDishFilteredAsync(MenuDishFilterRequest request, int page, int pageSize);
        Task<ApiResponse<MenuDishResponse>> CreateAsync(MenuDishCreateRequest request);
        Task<ApiResponse<MenuDishResponse>> UpdateAsync(int id, MenuDishUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
