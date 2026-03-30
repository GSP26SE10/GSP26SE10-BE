using BookfetSystem.Services.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IApiKeyProvider _apiKeyProvider;

    public GeminiService(HttpClient httpClient, IApiKeyProvider apiKeyProvider)
    {
        _httpClient = httpClient;
        _apiKeyProvider = apiKeyProvider;
    }

    public async Task<string> AskGemini(string prompt)
    {
        var currentKey = _apiKeyProvider.GetRandomKey();
        var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={currentKey}";

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

        // Prompt đã được tinh chỉnh để văn phong tự nhiên, không máy móc
        var prompt = $@"
    Bạn là một chuyên gia ẩm thực chuyên nghiệp.
    Tên món ăn: '{menuName}'
    
    Thông tin cũ để tham khảo: 
    ""{(string.IsNullOrEmpty(oldSummary) ? "Chưa có dữ liệu." : oldSummary)}""

    Danh sách các đánh giá mới nhất từ khách hàng:
    - {feedbackText}
    
    Nhiệm vụ: 
    Hãy viết một đoạn văn ngắn gọn (2-3 câu) tổng hợp chất lượng của món ăn này dựa trên tất cả thông tin trên.

    Yêu cầu quan trọng:
    1. Tuyệt đối KHÔNG sử dụng các cụm từ lặp lại máy móc như ""Tiếp tục được đánh giá"", ""Bản tóm tắt chất lượng là"", ""Dựa trên các đánh giá"".
    2. Hãy viết tự nhiên như một lời nhận xét trực tiếp (Ví dụ: ""{menuName} gây ấn tượng bởi..."" hoặc ""Món {menuName} khẳng định chất lượng nhờ..."").
    3. Kết hợp nhuần nhuyễn thông tin cũ và mới thành một mạch văn duy nhất, tập trung vào hương vị, trải nghiệm và mức độ hài lòng.
    4. Trả về kết quả thuần văn bản, không tiêu đề, không lời dẫn, không có ký tự đặc biệt.";

        return await AskGemini(prompt);
    }
    public async Task<string> SummarizeServiceFeedbackAsync(string serviceName, List<string> feedbacks, string? oldSummary)
    {
        var feedbackText = string.Join("\n- ", feedbacks);

        var prompt = $@"
    Bạn là chuyên gia quản lý chất lượng dịch vụ khách hàng chuyên nghiệp.
    Dịch vụ: '{serviceName}'
    
    Tóm tắt cũ: ""{(string.IsNullOrEmpty(oldSummary) ? "Chưa có" : oldSummary)}""
    Đánh giá mới:
    - {feedbackText}

    Nhiệm vụ: 
    Hãy tổng hợp thành một đoạn văn ngắn (2-3 câu) về chất lượng dịch vụ hiện tại.

    Yêu cầu bắt buộc:
    1. Tuyệt đối KHÔNG bao gồm các tiêu đề như ""Bản tóm tắt..."" hay bất kỳ lời dẫn nào.
    2. Đi thẳng trực tiếp vào nội dung nhận xét (ví dụ: ""Dịch vụ {serviceName} hiện đang..."").
    3. Tập trung vào: Thái độ, tốc độ, sự chuyên nghiệp và xu hướng thay đổi chất lượng.
    4. Văn phong khách quan, trả về thuần văn bản, không xuống dòng thừa.";

        return await AskGemini(prompt);
    }
}