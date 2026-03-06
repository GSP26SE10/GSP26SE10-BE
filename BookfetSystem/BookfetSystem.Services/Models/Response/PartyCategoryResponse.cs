namespace BookfetSystem.Services.Models.Response
{
    public class PartyCategoryResponse
    {
        public int PartyCategoryId { get; set; }

        public string? PartyCategoryName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public int? NumberOfGuests { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}