namespace BookfetSystem.Services.Models.Response
{
    public class StaffGroupResponse
    {
        public int StaffGroupId { get; set; }
        public string StaffGroupName { get; set; }
        public string Status { get; set; }
        public int? LeaderId { get; set; }
        public string LeaderName { get; set; }
    }
}

