namespace BookfetSystem.Services.Models.Request
{
    public class OrderDetailCustomCreateRequest
    {
        public int? OrderDetailId { get; set; }

        public int? DishId { get; set; }

        public int? Quantity { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class OrderDetailCustomUpdateRequest
    {
        public int OrderDetailCustomId { get; set; }

        public int? OrderDetailId { get; set; }

        public int? DishId { get; set; }

        public int? Quantity { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class OrderDetailCustomFilterRequest
    {
        public int? OrderDetailCustomId { get; set; }

        public int? OrderDetailId { get; set; }

        public int? DishId { get; set; }

        public int? Quantity { get; set; }
    }
}