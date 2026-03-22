namespace BookfetSystem.Services.Models.Response
{
    public class NotificationResponse
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public int Type { get; set; }
        public bool IsRead { get; set; }
        public bool IsSent { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
