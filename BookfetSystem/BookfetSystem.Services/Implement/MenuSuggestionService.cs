using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Implement
{
    public class MenuSuggestionService : IMenuSuggestionService
    {
        private readonly AISuggestionHandler _aiSuggestionHandler;

        public MenuSuggestionService(AISuggestionHandler aiSuggestionHandler)
        {
            _aiSuggestionHandler = aiSuggestionHandler;
        }

        public async Task<MenuSuggestionResponse> SuggestMenu(MenuSuggestionRequest request)
        {
            return await _aiSuggestionHandler.Handle(request);
        }
    }
}
