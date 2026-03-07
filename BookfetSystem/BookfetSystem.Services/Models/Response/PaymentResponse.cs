using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Response
{
    public class PaymentResponse
    {
        public int PaymentId { get; set; }

        public int? OrderId { get; set; }

        public decimal? Amount { get; set; }

        public string PaymentType { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
