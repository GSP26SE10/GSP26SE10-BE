namespace BookfetSystem.Services.Models.Response
{
    using BookfetSystem.Services.Models;

    public class StaffGroupAssignmentOverviewResponse
    {
        public StaffGroupAssignmentGroupResponse StaffGroup { get; set; } = new();
        public List<StaffGroupAssignmentOrderResponse> Orders { get; set; } = new();
    }

    public class StaffGroupAssignmentGroupResponse
    {
        public int StaffGroupId { get; set; }
        public string? StaffGroupName { get; set; }
        public StaffGroupAssignmentMemberResponse Leader { get; set; } = new();
        public List<StaffGroupAssignmentMemberResponse> Members { get; set; } = new();
    }

    public class StaffGroupAssignmentMemberResponse
    {
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
    }

    public class StaffGroupAssignmentOrderResponse
    {
        public int? OrderId { get; set; }
        public int OrderDetailId { get; set; }

        public StaffGroupAssignmentStatusResponse Status { get; set; } = new();
        public StaffGroupAssignmentPricingResponse Pricing { get; set; } = new();
        public StaffGroupAssignmentCustomerResponse Customer { get; set; } = new();
        public StaffGroupAssignmentMenuResponse Menu { get; set; } = new();
        public ServiceSnapshotDto? ServiceSnapshot { get; set; }
        public CustomDishSnapshotDto? CustomDishSnapshot { get; set; }
        public StaffGroupAssignmentPartyResponse Party { get; set; } = new();
        public StaffGroupAssignmentScheduleResponse Schedule { get; set; } = new();
        public List<StaffGroupAssignmentExtraChargeResponse> ExtraCharges { get; set; } = new();
        public List<StaffGroupAssignmentTaskResponse> Tasks { get; set; } = new();
    }

    public class StaffGroupAssignmentStatusResponse
    {
        public int? Order { get; set; }
        public int? OrderDetail { get; set; }
    }

    public class StaffGroupAssignmentPricingResponse
    {
        public decimal? TotalPrice { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public decimal? ExtraChargeTotal { get; set; }
    }

    public class StaffGroupAssignmentCustomerResponse
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }

    public class StaffGroupAssignmentMenuResponse
    {
        public string? Name { get; set; }
        public string? Image { get; set; }
    }

    public class StaffGroupAssignmentPartyResponse
    {
        public string? Category { get; set; }
        public int? NumberOfGuests { get; set; }
    }

    public class StaffGroupAssignmentScheduleResponse
    {
        public string? Address { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class StaffGroupAssignmentTaskResponse
    {
        public int TaskId { get; set; }
        public string? TaskName { get; set; }
        public int? Status { get; set; }
        public List<StaffGroupAssignmentTaskAssigneeResponse> Assignees { get; set; } = new();
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Note { get; set; }
    }

    public class StaffGroupAssignmentTaskAssigneeResponse
    {
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
    }

    public class StaffGroupAssignmentExtraChargeResponse
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public StaffGroupAssignmentExtraChargeCreatorResponse CreatedBy { get; set; } = new();
        public DateTime? IncurredAt { get; set; }
        public List<string> Images { get; set; } = new();
        public string? Note { get; set; }
    }

    public class StaffGroupAssignmentExtraChargeCreatorResponse
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}
