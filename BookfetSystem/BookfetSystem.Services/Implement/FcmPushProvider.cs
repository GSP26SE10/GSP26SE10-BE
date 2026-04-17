using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BookfetSystem.Services.Implement
{
    public class FcmPushProvider : IPushProvider
    {
        private const string FcmEndpoint = "https://fcm.googleapis.com/v1/projects/{0}/messages:send";
        private readonly FirebaseOptions _options;
        private readonly ILogger<FcmPushProvider> _logger;
        private GoogleCredential? _credential;

        public FcmPushProvider(IOptions<FirebaseOptions> options, ILogger<FcmPushProvider> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool SupportsTokenFormat(string token)
        {
            // FCM tokens are typically long hex strings, not Expo format
            return !token.StartsWith("ExponentPushToken[");
        }

        public async Task<bool> SendAsync(string token, string title, string body, NotificationType type, Dictionary<string, string>? data = null)
        {
            try
            {
                var credential = await GetCredentialAsync();
                if (credential == null)
                {
                    _logger.LogError("Failed to initialize Firebase credential");
                    return false;
                }

                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var payload = new FcmMessage
                {
                    Message = new FcmMessageContent
                    {
                        Token = token,
                        Notification = new FcmNotification
                        {
                            Title = title,
                            Body = body
                        },
                        Data = data ?? new Dictionary<string, string>()
                    }
                };

                payload.Message.Data["notificationType"] = ((int)type).ToString();

                var endpoint = string.Format(FcmEndpoint, _options.ProjectId);
                var response = await client.PostAsJsonAsync(endpoint, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("FCM push failed with status {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                        errorContent.Contains("NOT_FOUND") ||
                        errorContent.Contains("invalid registration"))
                    {
                        _logger.LogWarning("FCM token marked as unregistered: {Token}", token);
                        return false;
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send FCM push notification to token {Token}", token);
                return false;
            }
        }

        private async Task<GoogleCredential?> GetCredentialAsync()
        {
            if (_credential != null)
            {
                return _credential;
            }

            try
            {
                if (!string.IsNullOrEmpty(_options.CredentialsPath) && File.Exists(_options.CredentialsPath))
                {
                    _credential = GoogleCredential.FromFile(_options.CredentialsPath);
                }
                else
                {
                    _credential = GoogleCredential.GetApplicationDefault();
                }

                _credential = _credential?.CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                return _credential;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase credential from ADC or file");
                return null;
            }
        }

        private class FcmMessage
        {
            [JsonPropertyName("message")]
            public FcmMessageContent Message { get; set; } = new();
        }

        private class FcmMessageContent
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("notification")]
            public FcmNotification Notification { get; set; } = new();

            [JsonPropertyName("data")]
            public Dictionary<string, string> Data { get; set; } = new();
        }

        private class FcmNotification
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("body")]
            public string Body { get; set; } = string.Empty;
        }
    }
}
