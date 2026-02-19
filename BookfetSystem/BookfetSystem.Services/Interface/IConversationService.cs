using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IConversationService
    {
        Task<PagedResponse<ConversationResponse>> GetAllConversationFilteredAsync(ConversationFilterRequest request, int page, int pageSize);
        Task<ApiResponse<ConversationResponse>> CreateAsync(ConversationCreateRequest request);
        Task<ApiResponse<ConversationResponse>> UpdateAsync(int id, ConversationUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
