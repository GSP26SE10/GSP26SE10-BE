using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IContactRequestService
    {
        Task<PagedResponse<ContactRequestResponse>> GetAllFilteredAsync(ContactRequestFilterRequest request, int page, int pageSize);
        Task<ApiResponse<ContactRequestResponse>> CreateAsync(ContactRequestCreateRequest request);
        Task<ApiResponse<ContactRequestResponse>> UpdateAsync(int id, ContactRequestUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}