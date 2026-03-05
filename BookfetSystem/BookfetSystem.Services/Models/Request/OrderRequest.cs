using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderCreateRequest
    {
        [Required]
        public int CustomerId { get; set; }
    }

    public class OrderUpdateRequest
    {
        [Required]
        public string Status { get; set; }
    }

    public class OrderFilterRequest
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
