using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IMenuService
    {
        Task<PagedResponse<MenuResponse>> GetAllMenuFilteredAsync(MenuFilterRequest request, int page, int pageSize);
        Task<ApiResponse<MenuResponse>> CreateAsync(MenuCreateRequest request);
        Task<ApiResponse<MenuResponse>> UpdateAsync(int id, MenuUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}