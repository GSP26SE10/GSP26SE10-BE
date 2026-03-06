using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Request
{
    public class PaymentRequest
    {
        public class PaymentCreateRequest
        {
            public int? OrderId { get; set; }

            public decimal? Amount { get; set; }

            public string PaymentType { get; set; }

            public string PaymentMethod { get; set; }

            public string PaymentStatus { get; set; }

            public DateTime? PaidAt { get; set; }
        }
        
        public class PaymentUpdateRequest
        {
            public int? OrderId { get; set; }

            public decimal? Amount { get; set; }

            public string PaymentType { get; set; }

            public string PaymentMethod { get; set; }

            public string PaymentStatus { get; set; }

            public DateTime? PaidAt { get; set; }
        }

        public class PaymentFilterRequest
        {
            public int? OrderId { get; set; }

            public string PaymentStatus { get; set; }

            public string PaymentMethod { get; set; }
        }
    }
}