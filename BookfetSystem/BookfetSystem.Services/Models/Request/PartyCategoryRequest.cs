using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BookfetSystem.Services.Models.Request
{
    public class PartyCategoryCreateRequest
    {
        [Required]
        public string? PartyCategoryName { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "NumberOfGuests is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "NumberOfGuests must be greater than 0.")]
        public int NumberOfGuests { get; set; }

        public IFormFile? ImageUrl { get; set; }
    }

    public class PartyCategoryUpdateRequest
    {
        [Required]
        public string? PartyCategoryName { get; set; }

        public string? Description { get; set; }

        [EnumDataType(typeof(PartyCategoryStatus), ErrorMessage = "Invalid status value.")]
        public PartyCategoryStatus? Status { get; set; }

        [Required(ErrorMessage = "NumberOfGuests is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "NumberOfGuests must be greater than 0.")]
        public int NumberOfGuests { get; set; }

        public IFormFile? ImageUrl { get; set; }
    }

    public class PartyCategoryFilterRequest
    {
        public int PartyCategoryId { get; set; }

        public string? PartyCategoryName { get; set; }

        public PartyCategoryStatus? Status { get; set; }

        public int? NumberOfGuests { get; set; }
    }
}