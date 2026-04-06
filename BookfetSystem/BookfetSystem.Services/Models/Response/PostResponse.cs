using System;
using System.Collections.Generic;

namespace BookfetSystem.Services.Models.Response
{
    public class PostResponse
    {
        public int PostId { get; set; }
        public int? BlogCategoryId { get; set; }
        public string? BlogCategoryName { get; set; }
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public string? Excerpt { get; set; }
        public List<string>? CoverImage { get; set; }
        public string? Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
