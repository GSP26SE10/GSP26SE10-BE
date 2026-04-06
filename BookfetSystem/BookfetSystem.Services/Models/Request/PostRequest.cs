using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json;

namespace BookfetSystem.Services.Models.Request
{
    public class PostCreateRequest
    {
        [Required(ErrorMessage = "Slug is required.")]
        public string? Slug { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }

        public string? Excerpt { get; set; }

        /// <summary>
        /// Optional image files for cover image gallery (max 5 files).
        /// </summary>
        public List<IFormFile>? CoverImageFiles { get; set; }

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

        /// <summary>
        /// JSON array of image URLs for cover image gallery.
        /// </summary>
        public JsonElement? CoverImage { get; set; }

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
        /// <summary>
        /// Filter by status enum value: 0 = Draft, 1 = Published, 2 = Archived.
        /// </summary>
        public PostStatus? Status { get; set; }
    }
}
