namespace BookfetSystem.Services.Models.Response
{
    public class OrderResponse
    {
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

        public decimal? TotalPrice { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}