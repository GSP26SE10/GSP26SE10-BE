using System;

namespace BookfetSystem.Services.Models.Response
{
    public class MenuResponse
    {
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? BasePrice { get; set; }
        public object? ImgUrl { get; set; }
        public string? Status { get; set; }
        public string? MenuCategoryName { get; set; }
        public string? PartyCategoryName { get; set; }
    }
}