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
    public int? OrderDetailStatus { get; set; }
    public int? OrderStatus { get; set; }
        public decimal? ExtraChargeCost { get; set; }
        public List<StaffGroupAssignmentExtraChargeResponse> ExtraCharges { get; set; } = new();
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string? MenuName { get; set; }
        public string? MenuImage { get; set; }
        public string? PartyCategory { get; set; }
        public int? NumberOfGuests { get; set; }
        public string? Address { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<StaffGroupAssignmentTaskResponse> Tasks { get; set; } = new();
    }

    public class StaffGroupAssignmentExtraChargeResponse
    {
        public int OrderDetailExtraChargeId { get; set; }
        public int? ExtraChargeCatalogId { get; set; }
        public string? ChargeType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public int? CreateBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? IncurredAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public object? Image { get; set; }
        public string? Note { get; set; }
    }

    public class StaffGroupAssignmentTaskResponse
    {
        public int TaskId { get; set; }
        public string? TaskName { get; set; }
        public int? Status { get; set; }
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Note { get; set; }
    }
}
