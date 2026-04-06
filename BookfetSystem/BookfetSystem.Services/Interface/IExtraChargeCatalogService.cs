using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IExtraChargeCatalogService
    {
        Task<PagedResponse<ExtraChargeCatalogResponse>> GetAllFilteredAsync(ExtraChargeCatalogFilterRequest request, int page, int pageSize);
        Task<ApiResponse<ExtraChargeCatalogResponse>> CreateAsync(ExtraChargeCatalogCreateRequest request);
        Task<ApiResponse<ExtraChargeCatalogResponse>> UpdateAsync(int id, ExtraChargeCatalogUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
