using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderCreateRequest
    {
        public int? CustomerId { get; set; }

        public OrderStatus? Status { get; set; }
        public decimal? TotalPrice { get; set; }
    }

    public class OrderUpdateRequest
    {
        public int? CustomerId { get; set; }

        public OrderStatus? Status { get; set; }

        public decimal? TotalPrice { get; set; }
    }

    public class OrderFilterRequest
    {
        public int? OrderId { get; set; }
        public int? CustomerId { get; set; }
        public string? Status { get; set; }
    }
}
