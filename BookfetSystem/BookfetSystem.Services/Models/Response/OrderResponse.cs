namespace BookfetSystem.Services.Models.Response
{
    public class OrderResponse
    {
        public int OrderId { get; set; }

        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public int? Status { get; set; }

        public decimal? TotalPrice { get; set; }

        public decimal? DepositAmount { get; set; }

        public decimal? RemainingAmount { get; set; }

        public string? NoteOrder { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        public List<OrderDetailResponse> OrderDetails { get; set; } = new();
    }
}