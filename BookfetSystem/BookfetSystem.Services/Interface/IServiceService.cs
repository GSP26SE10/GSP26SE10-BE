using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interfaces
{
    public interface IServiceService
    {
        Task<PagedResponse<ServiceResponse>> GetAllFilteredAsync(ServiceFilterRequest filter, int page, int pageSize);

        Task<ServiceResponse?> GetById(int id);

        Task<ApiResponse<ServiceResponse>> Create(ServiceCreateRequest request);

        Task<ApiResponse<ServiceResponse>> Update(int id, ServiceUpdateRequest request);

        Task<ApiResponse<bool>> Delete(int id);
    }
}