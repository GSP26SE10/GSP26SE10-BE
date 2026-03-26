
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface IMenuSuggestionService
    {
        Task<ApiResponse<int>> SuggestMenu(MenuSuggestionRequest request);
    }
}