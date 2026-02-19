using System;

namespace BookfetSystem.Services.Models.Response
{
    public class ConversationResponse
    {
        public int ConversationId { get; set; }
        public int? CustomerId { get; set; }
        public int? OwnerId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public string OwnerName { get; set; }
    }
}
