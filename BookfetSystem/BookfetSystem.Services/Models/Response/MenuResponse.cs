using System;
using System.Collections.Generic;

namespace BookfetSystem.Services.Models.Response
{
    public class MenuResponse
    {
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? BasePrice { get; set; }
        public object? ImgUrl { get; set; }
        public int? Status { get; set; }
        public string? AisMenuSummary { get; set; }
        public string? MenuCategoryName { get; set; }
        public string? PartyCategoryName { get; set; }
        public int? ServiceDurationMinutes { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public List<int> PartyCategoryIds { get; set; } = new();
        public List<string> PartyCategoryNames { get; set; } = new();
        public List<int> ServiceDurationMinutesList { get; set; } = new();

    }
}