namespace BookfetSystem.Services.Models.Response
{
    public class PartyCategoryMenuResponse
    {
        public int PartyCategoryMenuId { get; set; }
        public int? PartyCategoryId { get; set; }
        public int? MenuId { get; set; }
        public string? PartyCategoryName { get; set; }
        public string? MenuName { get; set; }
    }
}
