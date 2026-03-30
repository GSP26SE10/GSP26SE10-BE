using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"];
    }

    public async Task<string> AskGemini(string prompt)
    {
        // Sử dụng bản v1beta và model gemini-2.5-flash
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            throw new Exception("Gemini API Rate Limit exceeded (429). Hangfire sẽ tự động thử lại sau.");
        }

        response.EnsureSuccessStatusCode(); // Ném ra lỗi nếu không phải 200 OK
        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);

        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
    }
    public async Task<string> SummarizeFeedbackAsync(string menuName, List<string> feedbacks)
    {
        var feedbackText = string.Join("\n- ", feedbacks);
        var prompt = $@"Bạn là một chuyên gia ẩm thực. Dưới đây là các đánh giá của khách hàng về món '{menuName}':
{feedbackText}

Hãy viết một bản tóm tắt ngắn gọn (khoảng 2-3 câu) về chất lượng món ăn này. 
Yêu cầu: Ngôn ngữ tự nhiên, khách quan, tập trung vào hương vị và cảm nhận chung. Trả về kết quả thuần văn bản.";

        return await AskGemini(prompt);
    }
}