using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Interface
{
    public interface IPushProvider
    {
        Task<bool> SendAsync(string token, string title, string body, NotificationType type, Dictionary<string, string>? data = null);
        bool SupportsTokenFormat(string token);
    }
}
