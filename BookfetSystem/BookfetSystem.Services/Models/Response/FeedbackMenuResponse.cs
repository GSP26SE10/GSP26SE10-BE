using System;

namespace BookfetSystem.Services.Models.Response
{
    public class FeedbackMenuResponse
    {
        public int FeedbackMenuId { get; set; }
        public int? MenuId { get; set; }
        public int? CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? MenuName { get; set; }
        public string? CustomerName { get; set; }
    }
}
