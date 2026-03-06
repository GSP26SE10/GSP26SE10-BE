using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IPartyCategoryService
    {
        Task<PagedResponse<PartyCategoryResponse>> GetAllPartyCategoryFilteredAsync(
            PartyCategoryFilterRequest request, int page, int pageSize);

        Task<ApiResponse<PartyCategoryResponse>> CreateAsync(PartyCategoryCreateRequest request);

        Task<ApiResponse<PartyCategoryResponse>> UpdateAsync(int id, PartyCategoryUpdateRequest request);

        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}