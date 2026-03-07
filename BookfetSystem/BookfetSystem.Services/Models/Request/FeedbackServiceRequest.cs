using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class FeedbackServiceCreateRequest
    {
        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be from 1 to 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class FeedbackServiceUpdateRequest
    {
        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be from 1 to 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [EnumDataType(typeof(FeedbackServiceStatus), ErrorMessage = "Invalid status value.")]
        public FeedbackServiceStatus? Status { get; set; }
    }

    public class FeedbackServiceFilterRequest
    {
        public int FeedbackServiceId { get; set; }
        public int? ServiceId { get; set; }
        public int? CustomerId { get; set; }
        public int? Rating { get; set; }
        public string? Status { get; set; }
        public string? Comment { get; set; }
    }
}
