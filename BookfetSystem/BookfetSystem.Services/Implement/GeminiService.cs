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
        // Sử dụng v1 và model gemini-1.5-flash để ổn định nhất (hoặc gemini-2.5-flash tùy bạn)
        var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var body = new
        {
            contents = new[]
            {
            new { parts = new[] { new { text = prompt } } }
        }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // Thử lại tối đa 3 lần nếu gặp lỗi Rate Limit
        for (int i = 0; i < 3; i++)
        {
            var response = await _httpClient.PostAsync(url, content);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Đợi một khoảng thời gian tăng dần trước khi thử lại (Exponential Backoff)
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i + 1)));
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini Error: {response.StatusCode} - {errorDetail}");
            }

            var result = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(result);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }

        throw new Exception("Gemini API Rate Limit exceeded after retries.");
    }
    public async Task<string> SummarizeFeedbackAsync(string menuName, List<string> feedbacks, string? oldSummary)
    {
        var feedbackText = string.Join("\n- ", feedbacks);

        // Logic Prompt thông minh: Kết hợp cũ và mới
        var prompt = $@"
        Bạn là một chuyên gia ẩm thực chuyên nghiệp.
        Tên món ăn: '{menuName}'
        
        Bản tóm tắt chất lượng hiện tại (nếu có): 
        ""{(string.IsNullOrEmpty(oldSummary) ? "Chưa có dữ liệu tóm tắt trước đó." : oldSummary)}""

        Danh sách các đánh giá mới nhất từ khách hàng:
        - {feedbackText}

        Nhiệm vụ: 
        Dựa trên bản tóm tắt cũ và các đánh giá mới này, hãy viết lại một bản tóm tắt tổng quan mới (khoảng 2-3 câu). 
        Yêu cầu:
        1. Nếu đánh giá mới có xu hướng thay đổi so với bản cũ, hãy cập nhật thông tin đó.
        2. Nếu đánh giá mới vẫn tương đồng, hãy tổng hợp lại cho súc tích hơn.
        3. Giữ phong cách chuyên nghiệp, khách quan, tập trung vào hương vị và trải nghiệm.
        4. Trả về kết quả thuần văn bản, không có lời dẫn hay ký tự đặc biệt.";

        return await AskGemini(prompt);
    }
}