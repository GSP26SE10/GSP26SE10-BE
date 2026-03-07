using BookfetSystem.Services.Models.Request;
using static BookfetSystem.Services.Models.Request.PaymentRequest;

namespace BookfetSystem.Services.Interface
{
    public interface IPaymentService
    {
        Task<object> GetAllFilteredAsync(PaymentFilterRequest filter, int page, int pageSize);

        Task<(bool Success, string Message)> CreateAsync(PaymentCreateRequest request);

        Task<(bool Success, string Message)> UpdateAsync(int id, PaymentUpdateRequest request);

        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}