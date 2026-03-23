using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface INotificationService
    {
        Task SendToUserAsync(int userId, string title, string body, NotificationType type, Dictionary<string, string>? data = null);
        Task<PagedResponse<NotificationResponse>> GetAllNotificationFilteredAsync(NotificationFilterRequest request, int userId, int page, int pageSize);
        Task<ApiResponse<NotificationResponse>> MarkAsReadAsync(int notificationId, int userId);
    }
}