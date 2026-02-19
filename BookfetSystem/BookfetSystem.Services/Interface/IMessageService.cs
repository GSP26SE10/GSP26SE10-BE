using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IMessageService
    {
        Task<PagedResponse<MessageResponse>> GetAllMessageFilteredAsync(MessageFilterRequest request, int page, int pageSize);
        Task<ApiResponse<MessageResponse>> CreateAsync(MessageCreateRequest request);
        Task<ApiResponse<MessageResponse>> UpdateAsync(int id, MessageUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
