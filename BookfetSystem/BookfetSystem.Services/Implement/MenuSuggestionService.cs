using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class MenuSuggestionService : IMenuSuggestionService
{
    private readonly MenuRepository _menuRepository;
    private readonly GeminiService _geminiService;

    public MenuSuggestionService(MenuRepository menuRepository, GeminiService geminiService)
    {
        _menuRepository = menuRepository;
        _geminiService = geminiService;
    }

    public async Task<ApiResponse<MenuSuggestionResponse>> SuggestMenu(MenuSuggestionRequest request)
    {
        try
        {
            var pricePerPerson = request.Budget / request.NumberOfGuests;

            var menus = await _menuRepository.GetAllWithRelationsAsync();

            // 🔥 1. Filter
            var filteredMenus = menus
                .Where(m =>
                    m.BasePrice.HasValue &&
                    Math.Abs(m.BasePrice.Value - pricePerPerson) <= 50000 &&
                    m.PartyCategoryMenus.Any(p => p.PartyCategoryId == request.PartyCategoryId)
                )
                .ToList();

            // 🔥 2. Filter Allergy
            if (request.AllergyDishes != null && request.AllergyDishes.Any())
            {
                filteredMenus = filteredMenus
                    .Where(m => !m.MenuDishes.Any(md =>
                        request.AllergyDishes.Any(a =>
                            md.Dish.DishName.ToLower().Contains(a.ToLower())
                        )))
                    .ToList();
            }

            if (!filteredMenus.Any())
            {
                filteredMenus = menus;
            }

            if (filteredMenus == null || !filteredMenus.Any())
            {
                return new ApiResponse<MenuSuggestionResponse>
                {
                    Success = false,
                    Code = 404,
                    Message = "No menus available"
                };
            }
            // 🔥 3. Build prompt
            var prompt = BuildPrompt(filteredMenus, request);

            // 🔥 4. Call AI
            var aiRaw = await _geminiService.AskGemini(prompt);

            Console.WriteLine("====== AI RAW RESPONSE ======");
            Console.WriteLine(aiRaw);
            Console.WriteLine("====== END AI RESPONSE ======");

            // 🔥 5. Parse Gemini response
            var aiResponse = ParseGeminiResponse(aiRaw);

            // 🔥 6. Validate
            if (aiResponse != null && aiResponse.MenuId > 0)
            {
                var matched = filteredMenus.FirstOrDefault(x => x.MenuId == aiResponse.MenuId);

                if (matched != null)
                {
                    var reason = aiResponse.Reason;

                    return new ApiResponse<MenuSuggestionResponse>
                    {
                        Success = true,
                        Message = "AI suggested menu successfully",
                        Data = new MenuSuggestionResponse
                        {
                            MenuId = matched.MenuId,
                            MenuName = matched.MenuName,
                            ImgUrl = GetFirstImage(matched.ImgUrl),
                            BasePrice = matched.BasePrice,
                            Title = BuildTitle(matched),
                            Reason = CleanReason(reason),
                            IsFromAI = true
                        }
                    };
                }
            }

            // 🔥 7. Fallback
            var randomMenu = filteredMenus[new Random().Next(filteredMenus.Count)];

            return new ApiResponse<MenuSuggestionResponse>
            {
                Success = true,
                Message = "Fallback random menu",
                Data = new MenuSuggestionResponse
                {
                    MenuId = randomMenu.MenuId,
                    Reason = "AI không trả đúng format hoặc không chọn được menu hợp lệ.",
                    IsFromAI = false
                }
            };
        }
        catch (UpstreamServiceUnavailableException)
        {
            return new ApiResponse<MenuSuggestionResponse>
            {
                Success = false,
                Code = 503,
                Message = "Service unavailable",
                Data = null
            };
        }
        catch (Exception)
        {
            return new ApiResponse<MenuSuggestionResponse>
            {
                Success = false,
                Code = 500,
                Message = "Internal server error",
                Data = null
            };
        }
    }

    // =========================
    // 🔥 BUILD PROMPT (QUAN TRỌNG)
    // =========================
    private string BuildPrompt(List<Menu> menus, MenuSuggestionRequest request)
    {
        var menuData = menus.Select(m => new
        {
            menuId = m.MenuId,
            name = m.MenuName,
            price = m.BasePrice,
            dishes = m.MenuDishes.Select(md => md.Dish.DishName).ToList()
        });

        return $@"
You are a food expert.

Customer:
- Favorite dishes: {string.Join(", ", request.FavoriteDishes ?? new List<string>())}
- Allergies: {string.Join(", ", request.AllergyDishes ?? new List<string>())}

Menus:
{JsonSerializer.Serialize(menuData)}

IMPORTANT:
- Choose EXACTLY one menuId from the list
- Do NOT create new menuId
- Avoid allergy dishes
- Prefer favorite dishes

🔥 IMPORTANT:
- reason MUST be in Vietnamese
- Use natural Vietnamese language
- Không được đề cập menuId, id hoặc bất kỳ thông tin kỹ thuật nào
- Không dùng ngoặc đơn () hoặc ghi chú giải thích
- Không viết kiểu giải thích như ""thể hiện qua...""
- Văn phong tự nhiên, trôi chảy, thân thiện
- Không giải thích logic phía sau (không nói “vì tên combo có...”)

Return ONLY JSON:
{{
  ""menuId"": number,
  ""reason"": ""string""
}}
";
    }

    // =========================
    // 🔥 PARSE GEMINI (FIX LỖI CHÍNH)
    // =========================
    private MenuSuggestionResponse ParseGeminiResponse(string raw)
{
    try
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        // 🔥 STEP 1: remove markdown trước
        var cleanedRaw = raw.Replace("```json", "")
                            .Replace("```", "")
                            .Trim();

        Console.WriteLine("====== CLEANED RAW ======");
        Console.WriteLine(cleanedRaw);
        Console.WriteLine("=========================");

        // 🔥 STEP 2: extract JSON object
        var start = cleanedRaw.IndexOf("{");
        var end = cleanedRaw.LastIndexOf("}");

        if (start < 0 || end < 0)
            return null;

        var json = cleanedRaw.Substring(start, end - start + 1);

        Console.WriteLine("====== FINAL JSON ======");
        Console.WriteLine(json);
        Console.WriteLine("=======================");

        // 🔥 STEP 3: deserialize
        return System.Text.Json.JsonSerializer.Deserialize<MenuSuggestionResponse>(
            json,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    catch (Exception ex)
    {
        Console.WriteLine("PARSE ERROR: " + ex.Message);
        return null;
    }
}
    
    private string CleanReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return reason;

        return reason
            .Replace("\"", "")   // bỏ dấu "
            .Replace("  ", " ")  // tránh double space
            .Trim();
    }
    private string BuildTitle(Menu menu)
    {
        if (menu == null) return "Gợi ý menu";

        var price = menu.BasePrice.HasValue
            ? $"{menu.BasePrice.Value:N0} VNĐ/người"
            : "Giá liên hệ";

        return $"✨ Gợi ý: {menu.MenuName} ({price})";
    }

    private string GetFirstImage(string imgUrl)
    {
        if (string.IsNullOrWhiteSpace(imgUrl))
            return null;

        try
        {
            // nếu là JSON array
            if (imgUrl.Trim().StartsWith("["))
            {
                var images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(imgUrl);
                return images?.FirstOrDefault();
            }

            // nếu là string thường
            return imgUrl;
        }
        catch
        {
            // fallback nếu parse lỗi
            return imgUrl;
        }
    }
}