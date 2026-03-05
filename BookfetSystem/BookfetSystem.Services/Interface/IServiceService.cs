using BookfetSystem.Services.Models.Request;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IServiceService
    {
        Task<object> GetAllServiceFilteredAsync(ServiceFilterRequest filter, int page, int pageSize);

        Task<object> CreateAsync(ServiceCreateRequest request);

        Task<object> UpdateAsync(int id, ServiceUpdateRequest request);

        Task<object> DeleteAsync(int id);
    }
}