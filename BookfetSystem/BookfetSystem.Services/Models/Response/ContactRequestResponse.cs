using System;

namespace BookfetSystem.Services.Models.Response
{
    public class ContactRequestResponse
    {
        public int ContactRequestId { get; set; }
        public int? CustomerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? CustomerName { get; set; }
    }
}