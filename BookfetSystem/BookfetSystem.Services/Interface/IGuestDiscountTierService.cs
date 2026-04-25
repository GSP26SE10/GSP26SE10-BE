using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IGuestDiscountTierService
    {
        Task<PagedResponse<GuestDiscountTierResponse>> GetAllFilteredAsync(GuestDiscountTierFilterRequest filter, int page, int pageSize);
        Task<ApiResponse<GuestDiscountTierResponse>> CreateAsync(GuestDiscountTierCreateRequest request);
        Task<ApiResponse<GuestDiscountTierResponse>> UpdateAsync(int id, GuestDiscountTierUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
