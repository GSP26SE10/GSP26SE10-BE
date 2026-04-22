using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderDetailCreateRequest
    {

        public int? OrderId { get; set; }

        public string Address { get; set; } = string.Empty;

        public int? NumberOfGuests { get; set; }

        public OrderDetailStatus? Status { get; set; }

        public decimal? TotalPrice { get; set; }

        public string Type { get; set; } = string.Empty;

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? StaffGroupId { get; set; }

        public int? PartyCategoryId { get; set; }

        public int? MenuId { get; set; }
    }

    public class OrderDetailUpdateRequest
    {
        public int? OrderId { get; set; }

        public string? Address { get; set; }

        public int? NumberOfGuests { get; set; }

        public OrderDetailStatus? Status { get; set; }

        public decimal? TotalPrice { get; set; }

        public string? Type { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? StaffGroupId { get; set; }

        public int? PartyCategoryId { get; set; }

        public int? MenuId { get; set; }
    }

    public class OrderDetailFilterRequest
    {
        public int? OrderDetailId { get; set; }

        public int? OrderId { get; set; }

        public OrderDetailStatus? Status { get; set; }

        public OrderDetailType? Type { get; set; }

        public int? MenuId { get; set; }

        public int? PartyCategoryId { get; set; }
    }

    public class OrderDetailActualEndTimeUpdateRequest
    {
        [Required(ErrorMessage = "ActualEndTime is required.")]
        public DateTime? ActualEndTime { get; set; }
    }
}