using System;

namespace BookfetSystem.Services.Models.Response
{
    public class FeedbackServiceResponse
    {
        public int FeedbackServiceId { get; set; }
        public int? OrderId { get; set; }
        public int? ServiceId { get; set; }
        public int? CustomerId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ServiceName { get; set; }
        public string? CustomerName { get; set; }
    }
}
