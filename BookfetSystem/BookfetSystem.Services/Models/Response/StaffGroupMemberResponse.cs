namespace BookfetSystem.Services.Models.Response
{
    public class StaffGroupMemberResponse
    {
        public int StaffGroupMemberId { get; set; }
        public int? StaffGroupId { get; set; }
        public int? StaffId { get; set; }
        public int? Status { get; set; }
        public string StaffName { get; set; }
        public string StaffGroupName { get; set; }
    }
}
