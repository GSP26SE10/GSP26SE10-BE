namespace BookfetSystem.Services.Models.Response
{
    public class ServiceResponse
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string Description { get; set; }

        public decimal? BasePrice { get; set; }

        public int? Status { get; set; }

        public string Img { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? AisServiceSummary { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }

    }
}