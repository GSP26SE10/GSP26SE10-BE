using System;

namespace BookfetSystem.Services.Models.Response
{
    public class OrderDetailStaffTaskResponse
    {
        public int TaskId { get; set; }
        public int? OrderDetailId { get; set; }
        public int? StaffId { get; set; }
        public string? TaskName { get; set; }
        public int? TaskStatus { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Note { get; set; }
        public string? Img { get; set; }
        public string? StaffName { get; set; }
    }
}
