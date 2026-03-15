using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class StaffGroupMemberCreateRequest
    {
        [Required(ErrorMessage = "StaffGroupId is required.")]
        public int StaffGroupId { get; set; }

        [Required(ErrorMessage = "StaffId is required.")]
        public int StaffId { get; set; }
    }

    public class StaffGroupMemberUpdateRequest
    {
        [Required(ErrorMessage = "StaffGroupId is required.")]
        public int StaffGroupId { get; set; }

        [Required(ErrorMessage = "StaffId is required.")]
        public int StaffId { get; set; }

        [EnumDataType(typeof(StaffGroupStatus), ErrorMessage = "Invalid status value.")]
        public StaffGroupStatus? Status { get; set; }
    }

    public class StaffGroupMemberFilterRequest
    {
        public int StaffGroupMemberId { get; set; }
        public int? StaffGroupId { get; set; }
        public int? StaffId { get; set; }
        /// <summary>Filter theo status: 1 = ACTIVE, 0 = INACTIVE (enum số, DB lưu string).</summary>
        public StaffGroupStatus? Status { get; set; }
    }
}
