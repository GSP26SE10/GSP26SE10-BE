using System.Text;
using System.Text.Json;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using Microsoft.Extensions.Logging;

namespace BookfetSystem.Services.Implement
{
    public class ExpoPushProvider : IPushProvider
    {
        private const string ExpoPushSendEndpoint = "https://exp.host/--/api/v2/push/send";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExpoPushProvider> _logger;

        public ExpoPushProvider(IHttpClientFactory httpClientFactory, ILogger<ExpoPushProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public bool SupportsTokenFormat(string token)
        {
            return token.StartsWith("ExponentPushToken[") && token.EndsWith("]");
        }

        public async Task<bool> SendAsync(string token, string title, string body, NotificationType type, Dictionary<string, string>? data = null)
        {
            var payloadData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["notificationType"] = ((int)type).ToString()
            };

            if (data != null)
            {
                foreach (var item in data)
                {
                    payloadData[item.Key] = item.Value;
                }
            }

            var payload = new
            {
                to = token,
                title,
                body,
                data = payloadData
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(ExpoPushSendEndpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Expo push failed with status {StatusCode}. Response: {Response}", response.StatusCode, responseText);
                    return false;
                }

                if (ContainsDeviceNotRegistered(responseText))
                {
                    _logger.LogWarning("Expo token marked as unregistered: {Token}", token);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Expo push notification to token {Token}", token);
                return false;
            }
        }

        private static bool ContainsDeviceNotRegistered(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(responseText);
                if (!document.RootElement.TryGetProperty("data", out var dataElement) || dataElement.ValueKind != JsonValueKind.Array)
                {
                    return false;
                }

                foreach (var item in dataElement.EnumerateArray())
                {
                    if (!item.TryGetProperty("details", out var detailsElement) || detailsElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    if (!detailsElement.TryGetProperty("error", out var errorElement))
                    {
                        continue;
                    }

                    var errorValue = errorElement.GetString();
                    if (string.Equals(errorValue, "DeviceNotRegistered", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return responseText.Contains("DeviceNotRegistered", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
