using BookfetSystem.Services.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderDetailStaffTaskCreateRequest
    {
        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "StaffId is required.")]
        public int StaffId { get; set; }

        [MaxLength(255)]
        public string? TaskName { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Note { get; set; }
    }

    public class OrderDetailStaffTaskUpdateRequest
    {
        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "StaffId is required.")]
        public int StaffId { get; set; }

        [MaxLength(255)]
        public string? TaskName { get; set; }

        [EnumDataType(typeof(StaffTaskStatus), ErrorMessage = "Invalid task status. Use 1 for PENDING, 2 for COMPLETED, 3 for CANCELLED.")]
        public StaffTaskStatus? TaskStatus { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Note { get; set; }
    }

    public class OrderDetailStaffTaskFilterRequest
    {
        public int TaskId { get; set; }
        public int? OrderDetailId { get; set; }
        public int? StaffId { get; set; }
        public string? TaskName { get; set; }
        public string? TaskStatus { get; set; }
    }
}
