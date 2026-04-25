namespace BookfetSystem.Services.Models.Response
{
    public class GuestDiscountTierResponse
    {
        public int GuestDiscountTierId { get; set; }

        public int MinGuestCount { get; set; }

        public decimal DiscountPercent { get; set; }

        public string? Note { get; set; }

        public int? Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
