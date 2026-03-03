using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class BlogCategoryCreateRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Slug is required.")]
        public string? Slug { get; set; }
    }

    public class BlogCategoryUpdateRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Slug is required.")]
        public string? Slug { get; set; }
    }

    public class BlogCategoryFilterRequest
    {
        public int BlogCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
    }
}
