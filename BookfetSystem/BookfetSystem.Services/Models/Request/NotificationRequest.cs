using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class NotificationFilterRequest
    {
        public int NotificationId { get; set; }
        public NotificationType? Type { get; set; }
        public bool? IsRead { get; set; }
    }
}
