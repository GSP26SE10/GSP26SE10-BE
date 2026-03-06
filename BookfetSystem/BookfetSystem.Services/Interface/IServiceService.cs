using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interfaces
{
    public interface IServiceService
    {
        Task<List<ServiceResponse>> GetAll(ServiceFilterRequest filter);

        Task<ServiceResponse?> GetById(int id);

        Task<bool> Create(ServiceCreateRequest request);

        Task<bool> Update(ServiceUpdateRequest request);

        Task<bool> Delete(int id);
    }
}