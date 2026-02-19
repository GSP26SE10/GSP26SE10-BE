using System;

namespace BookfetSystem.Services.Models.Response
{
    public class MessageResponse
    {
        public int MessageId { get; set; }
        public int? ConversationId { get; set; }
        public int? SenderId { get; set; }
        public string Content { get; set; }
        public DateTime? SentAt { get; set; }
        public string SenderName { get; set; }
    }
}
