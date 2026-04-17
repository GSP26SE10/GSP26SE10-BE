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
        private readonly NotificationRepository _notificationRepository;
        private readonly UserDeviceRepository _userDeviceRepository;
        private readonly ExpoPushProvider _expoProvider;
        private readonly FcmPushProvider _fcmProvider;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationRepository notificationRepository,
            UserDeviceRepository userDeviceRepository,
            ExpoPushProvider expoProvider,
            FcmPushProvider fcmProvider,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _userDeviceRepository = userDeviceRepository;
            _expoProvider = expoProvider;
            _fcmProvider = fcmProvider;
            _logger = logger;
        }

        public async Task SendToUserAsync(int userId, string title, string body, NotificationType type, Dictionary<string, string>? data = null)
        {
            await SaveInAppNotificationAsync(userId, title, body, type);

            var devices = await _userDeviceRepository.GetActiveDevicesWithPlatformAsync(userId);
            if (!devices.Any())
            {
                return;
            }

            foreach (var (token, platform, deviceId) in devices)
            {
                await SendToPlatformAsync(token, platform, deviceId, title, body, type, data);
            }
        }

        private async Task SendToPlatformAsync(string token, string platform, int deviceId, string title, string body, NotificationType type, Dictionary<string, string>? data)
        {
            bool success = false;

            try
            {
                platform = platform?.ToLowerInvariant() ?? "ios";

                if (platform == "android")
                {
                    success = await _fcmProvider.SendAsync(token, title, body, type, data);
                }
                else
                {
                    // Default to Expo for iOS and unknown platforms
                    success = await _expoProvider.SendAsync(token, title, body, type, data);
                }

                if (!success)
                {
                    await _userDeviceRepository.DeactivateByExpoPushTokenAsync(token);
                    _logger.LogWarning("Device token deactivated due to send failure. DeviceId: {DeviceId}, Platform: {Platform}", deviceId, platform);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push to device {DeviceId} on platform {Platform}", deviceId, platform);
                await _userDeviceRepository.DeactivateByExpoPushTokenAsync(token);
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
    }
}