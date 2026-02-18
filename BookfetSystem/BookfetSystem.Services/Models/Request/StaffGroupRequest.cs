using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class StaffGroupCreateRequest
    {
        [Required(ErrorMessage = "StaffGroupName is required.")]
        [MaxLength(100, ErrorMessage = "StaffGroupName must be at most 100 characters.")]
        public string? StaffGroupName { get; set; }

        [Required(ErrorMessage = "LeaderId is required.")]
        public int LeaderId { get; set; }
    }

    public class StaffGroupUpdateRequest
    {
        [Required(ErrorMessage = "StaffGroupName is required.")]
        [MaxLength(100, ErrorMessage = "StaffGroupName must be at most 100 characters.")]
        public string? StaffGroupName { get; set; }

        [Required(ErrorMessage = "LeaderId is required.")]
        public int LeaderId { get; set; }

        [EnumDataType(typeof(StaffGroupStatus), ErrorMessage = "Invalid status value.")]
        public StaffGroupStatus? Status { get; set; }
    }

    public class StaffGroupFilterRequest
    {
        public int StaffGroupId { get; set; }
        public string? StaffGroupName { get; set; }
        public string? Status { get; set; }
        public int? LeaderId { get; set; }
    }
}

