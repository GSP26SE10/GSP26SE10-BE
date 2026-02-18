using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IRoleService
    {
        Task<PagedResponse<RoleResponse>> GetAllRoleFilteredAsync(RoleFilterRequest request, int page, int pageSize);
        Task<ApiResponse<RoleResponse>> CreateAsync(RoleCreateRequest request);
        Task<ApiResponse<RoleResponse>> UpdateAsync(int id, RoleUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}

