using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text;

public class AIMenuRecommendationService : IAIMenuRecommendationService
{
    private readonly IConfiguration _config;

    public AIMenuRecommendationService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> RecommendMenuAsync(AIMenuRequest request)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        var client = new ChatClient(model: "gpt-4o-mini", apiKey: apiKey);

        var prompt = BuildPrompt(request);

        var response = await client.CompleteChatAsync(prompt);

        return response.Value.Content[0].Text;
    }

    private string BuildPrompt(AIMenuRequest request)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Số khách: {request.GuestCount}");
        sb.AppendLine($"Loại tiệc: {request.PartyType}");
        sb.AppendLine($"Ngân sách: {request.Budget}");

        sb.AppendLine($"Món ưa thích: {string.Join(", ", request.PreferredDishes)}");
        sb.AppendLine($"Dị ứng: {string.Join(", ", request.Allergies)}");

        sb.AppendLine("Danh sách menu:");

        foreach (var menu in request.Menus)
        {
            sb.AppendLine($"Menu: {menu.MenuName} - Giá: {menu.BasePrice}");

            foreach (var md in menu.MenuDishes)
            {
                sb.AppendLine($"- {md.Dish?.DishName}");
            }
        }

        sb.AppendLine("Hãy chọn menu phù hợp nhất và đề xuất thêm món nếu cần.");

        return sb.ToString();
    }
}