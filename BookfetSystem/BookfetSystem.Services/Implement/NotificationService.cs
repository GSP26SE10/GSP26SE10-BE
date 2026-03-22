using System.Text;
using System.Text.Json;
using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookfetSystem.Services.Implement
{
    public class NotificationService : INotificationService
    {
        private const string ExpoPushSendEndpoint = "https://exp.host/--/api/v2/push/send";

        private readonly NotificationRepository _notificationRepository;
        private readonly UserDeviceRepository _userDeviceRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationRepository notificationRepository,
            UserDeviceRepository userDeviceRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _userDeviceRepository = userDeviceRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SendToUserAsync(int userId, string title, string body, NotificationType type, Dictionary<string, string>? data = null)
        {
            await SaveInAppNotificationAsync(userId, title, body, type);

            var tokens = await _userDeviceRepository.GetActiveTokensByUserIdAsync(userId);
            if (!tokens.Any())
            {
                return;
            }

            var client = _httpClientFactory.CreateClient();
            foreach (var token in tokens)
            {
                await SendOnePushAsync(client, token, title, body, type, data);
            }
        }

        public async Task<PagedResponse<NotificationResponse>> GetAllNotificationFilteredAsync(NotificationFilterRequest request, int userId, int page, int pageSize)
        {

            var entityFilter = request.Adapt<Notification>();
            var query = _notificationRepository.GetAllNotificationFiltered(entityFilter, userId);
            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<NotificationResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<NotificationResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<NotificationResponse>> MarkAsReadAsync(int notificationId, int userId)
        {
            var entity = await _notificationRepository.GetByIdAndUserIdAsync(notificationId, userId);
            if (entity == null)
            {
                return new ApiResponse<NotificationResponse>
                {
                    Success = false,
                    Message = "Notification not found.",
                    Data = null
                };
            }

            entity.IsRead = true;
            var updated = await _notificationRepository.UpdateAsync(entity);
            if (updated > 0)
            {
                return new ApiResponse<NotificationResponse>
                {
                    Success = true,
                    Message = "Marked as read successfully.",
                    Data = entity.Adapt<NotificationResponse>()
                };
            }

            return new ApiResponse<NotificationResponse>
            {
                Success = false,
                Message = "Failed to mark as read.",
                Data = null
            };
        }

        private async Task SaveInAppNotificationAsync(int userId, string title, string body, NotificationType type)
        {
            var entity = new Notification
            {
                UserId = userId,
                Title = title,
                Content = body,
                Type = ((int)type).ToString(),
                IsRead = false,
                IsSent = true,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(entity);
        }

        private async Task SendOnePushAsync(HttpClient client, string token, string title, string body, NotificationType type, Dictionary<string, string>? data)
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
                var response = await client.PostAsync(ExpoPushSendEndpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Expo push failed with status {StatusCode}. Response: {Response}", response.StatusCode, responseText);
                }

                if (ContainsDeviceNotRegistered(responseText))
                {
                    await _userDeviceRepository.DeactivateByExpoPushTokenAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Expo push notification to token {Token}", token);
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