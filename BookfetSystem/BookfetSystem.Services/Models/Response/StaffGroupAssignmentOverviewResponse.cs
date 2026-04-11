namespace BookfetSystem.Services.Models.Response
{
    public class StaffGroupAssignmentOverviewResponse
    {
        public int StaffGroupId { get; set; }
        public string? StaffGroupName { get; set; }
        public int? LeaderId { get; set; }
        public string? LeaderName { get; set; }
        public List<StaffGroupAssignmentMemberResponse> Members { get; set; } = new();
        public List<StaffGroupAssignmentOrderResponse> Orders { get; set; } = new();
    }

    public class StaffGroupAssignmentMemberResponse
    {
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
    }

    public class StaffGroupAssignmentOrderResponse
    {
        public int OrderDetailId { get; set; }
        public int? MenuId { get; set; }
        public string? MenuName { get; set; }
        public string? PartyCategory { get; set; }
        public int? NumberOfGuests { get; set; }
        public string? Address { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<StaffGroupAssignmentTaskResponse> Tasks { get; set; } = new();
    }

    public class StaffGroupAssignmentTaskResponse
    {
        public int TaskId { get; set; }
        public string? TaskName { get; set; }
        public string? Status { get; set; }
    }
}
