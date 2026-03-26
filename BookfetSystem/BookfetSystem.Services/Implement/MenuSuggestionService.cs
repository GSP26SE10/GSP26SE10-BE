using BookfetSystem.Repositories;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using Microsoft.EntityFrameworkCore;

public class MenuSuggestionService : IMenuSuggestionService
{
    private readonly MenuRepository _menuRepository;
    private readonly GeminiService _geminiService;

    public MenuSuggestionService(MenuRepository menuRepository, GeminiService geminiService)
    {
        _menuRepository = menuRepository;
        _geminiService = geminiService;
    }

    public async Task<ApiResponse<int>> SuggestMenu(MenuSuggestionRequest request)
    {
        try
        {
            var pricePerPerson = request.Budget / request.NumberOfGuests;

            var menus = await _menuRepository.GetAllWithRelationsAsync();

            // 🔥 1. Filter theo PartyCategory + Price
            var filteredMenus = menus
                .Where(m =>
                    m.BasePrice.HasValue &&
                    Math.Abs(m.BasePrice.Value - pricePerPerson) <= 50000 // tolerance
                    && m.PartyCategoryMenus.Any(p => p.PartyCategoryId == request.PartyCategoryId)
                )
                .ToList();

            // 🔥 2. Filter Allergy (loại bỏ menu có món dị ứng)
            if (request.AllergyDishes != null && request.AllergyDishes.Any())
            {
                filteredMenus = filteredMenus
                    .Where(m => !m.MenuDishes.Any(md =>
                        request.AllergyDishes.Any(a =>
                            md.Dish.DishName.ToLower().Contains(a.ToLower())
                        )))
                    .ToList();
            }

            // 🔥 Nếu không còn menu → fallback ALL
            if (!filteredMenus.Any())
            {
                filteredMenus = menus;
            }

            // 🔥 3. Chuẩn bị data cho Gemini
            var menuData = filteredMenus.Select(m => new
            {
                m.MenuId,
                m.MenuName,
                m.BasePrice,
                Dishes = m.MenuDishes.Select(md => md.Dish.DishName).ToList()
            });

            var prompt = $@"
Bạn là AI gợi ý menu tiệc.

Khách hàng:
- Sở thích: {string.Join(", ", request.FavoriteDishes ?? new List<string>())}
- Dị ứng: {string.Join(", ", request.AllergyDishes ?? new List<string>())}

Danh sách menu:
{System.Text.Json.JsonSerializer.Serialize(menuData)}

Yêu cầu:
- Chọn ra đúng 1 MenuId phù hợp nhất
- Chỉ trả về số MenuId, không giải thích
";

            var aiResult = await _geminiService.AskGemini(prompt);

            // 🔥 4. Parse kết quả
            if (int.TryParse(aiResult.Trim(), out int menuId))
            {
                return new ApiResponse<int>
                {
                    Success = true,
                    Message = "AI suggested menu successfully",
                    Data = menuId
                };
            }

            // 🔥 5. Fallback random
            var randomMenu = filteredMenus[new Random().Next(filteredMenus.Count)];

            return new ApiResponse<int>
            {
                Success = true,
                Message = "Fallback random menu",
                Data = randomMenu.MenuId
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<int>
            {
                Success = false,
                Message = ex.Message,
                Data = 0
            };
        }
    }
}