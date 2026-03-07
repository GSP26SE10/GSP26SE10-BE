namespace BookfetSystem.Services.Models.Request
{
    public class OrderDetailRequest
    {
        public int? OrderDetailId { get; set; }

        public int? OrderId { get; set; }

        public string Address { get; set; }

        public int? NumberOfGuests { get; set; }

        public string Status { get; set; }

        public decimal? TotalPrice { get; set; }

        public string Type { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? StaffGroupId { get; set; }

        public int? PartyCategoryId { get; set; }

        public int? MenuId { get; set; }
    }
}