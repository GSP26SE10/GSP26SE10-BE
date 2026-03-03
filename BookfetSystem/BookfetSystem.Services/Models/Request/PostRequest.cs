using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class PostCreateRequest
    {
        [Required(ErrorMessage = "Slug is required.")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }

        public string? Excerpt { get; set; }

        public int? CoverImageId { get; set; }

        [EnumDataType(typeof(PostStatus), ErrorMessage = "Invalid status value.")]
        public PostStatus Status { get; set; }

        [Required(ErrorMessage = "BlogCategoryId is required.")]
        public int BlogCategoryId { get; set; }
    }

    public class PostUpdateRequest
    {
        [Required(ErrorMessage = "Slug is required.")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }

        public string? Excerpt { get; set; }

        public int? CoverImageId { get; set; }

        [EnumDataType(typeof(PostStatus), ErrorMessage = "Invalid status value.")]
        public PostStatus Status { get; set; }

        [Required(ErrorMessage = "BlogCategoryId is required.")]
        public int BlogCategoryId { get; set; }
    }

    public class PostFilterRequest
    {
        public int PostId { get; set; }
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public int? BlogCategoryId { get; set; }
        public string? Status { get; set; }
    }
}
