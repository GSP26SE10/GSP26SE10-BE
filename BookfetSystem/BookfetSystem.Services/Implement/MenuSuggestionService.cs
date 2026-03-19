using BookfetSystem.Repositories;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class MenuSuggestionService : IMenuSuggestionService
    {
        private readonly MenuRepository _menuRepo;
        private readonly IAIMenuRecommendationService _aiService;

        public MenuSuggestionService(
            MenuRepository menuRepo,
            IAIMenuRecommendationService aiService)
        {
            _menuRepo = menuRepo;
            _aiService = aiService;
        }

        public async Task<string> SuggestMenu(AIMenuRequest request)
        {
            var menus = await _menuRepo.GetMenusWithDishAsync();

            request.Menus = menus;

            return await _aiService.RecommendMenuAsync(request);
        }
    }
}
