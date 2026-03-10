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

    // FOR ORDER CREATE BUSSINESS LOGIC
    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }

        public string Address { get; set; }

        public int NumberOfGuests { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int MenuId { get; set; }

        public int PartyCategoryId { get; set; }

        public List<ServiceItemRequest> Services { get; set; }
    }

    public class ServiceItemRequest
    {
        public int ServiceId { get; set; }

        public int Quantity { get; set; }
    }
}
