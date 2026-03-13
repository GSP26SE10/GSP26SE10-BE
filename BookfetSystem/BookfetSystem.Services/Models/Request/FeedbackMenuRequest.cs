using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class FeedbackMenuCreateRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "MenuId is required.")]
        public int MenuId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be from 1 to 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class FeedbackMenuUpdateRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "MenuId is required.")]
        public int MenuId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be from 1 to 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [EnumDataType(typeof(FeedbackMenuStatus), ErrorMessage = "Invalid status value.")]
        public FeedbackMenuStatus? Status { get; set; }
    }

    public class FeedbackMenuFilterRequest
    {
        public int FeedbackMenuId { get; set; }
        public int? OrderId { get; set; }
        public int? MenuId { get; set; }
        public int? CustomerId { get; set; }
        public int? Rating { get; set; }
        public string? Status { get; set; }
        public string? Comment { get; set; }
    }
}
