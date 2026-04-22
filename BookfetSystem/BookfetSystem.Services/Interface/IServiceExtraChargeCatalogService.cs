using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IServiceExtraChargeCatalogService
    {
        Task<PagedResponse<ServiceExtraChargeCatalogResponse>> GetAllFilteredAsync(ServiceExtraChargeCatalogFilterRequest filter, int page, int pageSize);
        Task<ApiResponse<ServiceExtraChargeCatalogResponse>> CreateAsync(ServiceExtraChargeCatalogCreateRequest request);
        Task<ApiResponse<ServiceExtraChargeCatalogResponse>> UpdateAsync(int id, ServiceExtraChargeCatalogUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
