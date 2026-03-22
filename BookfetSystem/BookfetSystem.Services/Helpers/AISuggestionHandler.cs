using BookfetSystem.Repositories;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Helpers
{
    public class AISuggestionHandler
    {
        private readonly MenuRepository _menuRepository;
        private readonly IAIRecommendationService _aiService;

        public AISuggestionHandler(
            MenuRepository menuRepository,
            IAIRecommendationService aiService)
        {
            _menuRepository = menuRepository;
            _aiService = aiService;
        }

        public async Task<MenuSuggestionResponse> Handle(MenuSuggestionRequest request)
        {
            // 1. Lấy menu từ DB
            var menus = await _menuRepository.GetAllWithRelationsAsync();

            var menuData = menus.Select(m => new
            {
                m.MenuId,
                m.MenuName,
                Dishes = m.MenuDishes.Select(md => md.Dish.DishName).ToList()
            });

            // 2. Tạo prompt
            var prompt = $@"
Dữ liệu menu:
{JsonSerializer.Serialize(menuData)}

Khách hàng:
- Số khách: {request.GuestCount}
- Loại tiệc: {request.PartyType}
- Ngân sách: {request.Budget}
- Món thích: {string.Join(", ", request.FavoriteDishes)}
- Dị ứng: {string.Join(", ", request.Allergies)}

Chọn 1 menu phù hợp nhất và trả về JSON:
{{
  ""menuId"": number,
  ""menuName"": ""string"",
  ""recommendedDishes"": [""string""],
  ""extraDishes"": [""string""],
  ""reason"": ""string""
}}
";

            // 3. Call AI
            var aiResult = await _aiService.GetRecommendationAsync(prompt);

            // 4. Parse JSON
            var cleanJson = aiResult.Substring(aiResult.IndexOf("{"));

            return JsonSerializer.Deserialize<MenuSuggestionResponse>(cleanJson);
        }
    }
}