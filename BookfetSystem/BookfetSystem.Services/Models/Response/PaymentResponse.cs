using System;

namespace BookfetSystem.Services.Models.Response
{
    public class PaymentResponse
    {
        public int PaymentId { get; set; }

        public int? OrderId { get; set; }

        public decimal? Amount { get; set; }

        public int? PaymentType { get; set; }

        public int? PaymentMethod { get; set; }

        public int? PaymentStatus { get; set; }

        public DateTime? PaidAt { get; set; }

        public object? MtdZlp { get; set; }
    }
}
