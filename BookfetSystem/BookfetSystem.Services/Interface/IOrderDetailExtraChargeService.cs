using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderDetailExtraChargeService
    {
        Task<ApiResponse<OrderDetailExtraChargeResponse>> CreateAsync(OrderDetailExtraChargeCreateRequest request, int leaderId);
        Task<List<ExtraChargeCatalogResponse>> GetActiveCatalogAsync();
    }
}
