using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class PartyCategoryCreateRequest
    {
        [Required]
        public string? PartyCategoryName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public int? NumberOfGuests { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class PartyCategoryUpdateRequest
    {
        [Required]
        public string? PartyCategoryName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public int? NumberOfGuests { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class PartyCategoryFilterRequest
    {
        public int PartyCategoryId { get; set; }

        public string? PartyCategoryName { get; set; }

        public MenuStatus? Status { get; set; }

        public int? NumberOfGuests { get; set; }
    }
}