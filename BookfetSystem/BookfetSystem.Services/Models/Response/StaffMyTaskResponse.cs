using System;
using BookfetSystem.Services.Models;

namespace BookfetSystem.Services.Models.Response
{
    public class StaffMyTaskResponse
    {
        public int TaskId { get; set; }
        public string? TaskName { get; set; }
        public int? TaskStatus { get; set; }
        public DateTime? TaskStartTime { get; set; }
        public DateTime? TaskEndTime { get; set; }
        public string? Note { get; set; }

        public StaffMyTaskOrderDetailResponse OrderDetail { get; set; } = new();
    }

    public class StaffMyTaskOrderDetailResponse
    {
        public int OrderDetailId { get; set; }
        public int? MenuId { get; set; }
        public string? MenuName { get; set; }
        public string? MenuImage { get; set; }
        public ServiceSnapshotDto? ServiceSnapshot { get; set; }
        public CustomDishSnapshotDto? CustomDishSnapshot { get; set; }
        public string? PartyCategory { get; set; }
        public int? ServiceDurationMinutes { get; set; }
        public int? NumberOfGuests { get; set; }
        public string? Address { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Status { get; set; }
        public int? OrderStatus { get; set; }
    }
}
