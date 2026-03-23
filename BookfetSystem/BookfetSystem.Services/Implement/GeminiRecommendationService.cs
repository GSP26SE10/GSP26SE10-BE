using BookfetSystem.Services.Interface;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class GeminiRecommendationService : IAIRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiRecommendationService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["Gemini:ApiKey"];
    }

    public async Task<string> GetRecommendationAsync(string prompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

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

        var json = JsonSerializer.Serialize(body);

        var response = await _httpClient.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine("🔥 GEMINI RESPONSE:");
        Console.WriteLine(responseString);

        using var doc = JsonDocument.Parse(responseString);

        var root = doc.RootElement;

        // ❌ Nếu có error → throw ra luôn
        if (root.TryGetProperty("error", out var error))
        {
            var message = error.GetProperty("message").GetString();
            throw new Exception($"Gemini API Error: {message}");
        }

        // ❌ Nếu không có candidates
        if (!root.TryGetProperty("candidates", out var candidates))
        {
            throw new Exception("Gemini response không có candidates");
        }

        return candidates[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
    }
}