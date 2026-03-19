using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Interface
{
    public interface INotificationService
    {
        Task SendToUserAsync(int userId, string title, string body, NotificationType type, Dictionary<string, string>? data = null);
    }
}