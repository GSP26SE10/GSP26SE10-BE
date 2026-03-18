using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IPaymentService
    {
        Task<PagedResponse<PaymentResponse>> GetAllPaymentFilteredAsync(PaymentFilterRequest request, int page, int pageSize);
        Task<ApiResponse<PaymentResponse>> CreateAsync(PaymentCreateRequest request);
        Task<ApiResponse<PaymentResponse>> UpdateAsync(int id, PaymentUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<object>> CreateDepositQR(int orderId);
        Task<ApiResponse<object>> CreateFullQR(int orderId);
        Task<ApiResponse<object>> CreateFullCashPayment(int orderId);
    }
}
