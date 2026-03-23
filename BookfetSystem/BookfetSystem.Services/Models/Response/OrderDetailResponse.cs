using BookfetSystem.Services.Models;

namespace BookfetSystem.Services.Models.Response
{
    public class OrderDetailResponse
    {
        public int OrderDetailId { get; set; }

        public int? OrderId { get; set; }

        public string? Address { get; set; }

        public int? NumberOfGuests { get; set; }

        public int? Status { get; set; }

        public decimal? TotalPrice { get; set; }

        public int? Type { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? StaffGroupId { get; set; }

        public int? PartyCategoryId { get; set; }

        public int? MenuId { get; set; }

        public string? MenuName { get; set; }

        public string? PartyCategoryName { get; set; }

        public MenuSnapshotDto? MenuSnapshot { get; set; }

        public ServiceSnapshotDto? ServiceSnapshot { get; set; }

        public CustomDishSnapshotDto? CustomDishSnapshot { get; set; }

        public string? NoteOrderDetail { get; set; }

        public decimal? ExtraChargeCost { get; set; }
    }
}