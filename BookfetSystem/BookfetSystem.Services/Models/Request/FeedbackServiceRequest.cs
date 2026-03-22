using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BookfetSystem.Services.Models.Request
{
    public class FeedbackServiceCreateRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be from 1 to 5.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public List<IFormFile>? ImgFiles { get; set; }
    }

    public class FeedbackServiceUpdateRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public int OrderId { get; set; }

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
        public int? OrderId { get; set; }
        public int? OrderDetailId { get; set; }
        public int? ServiceId { get; set; }
        public int? CustomerId { get; set; }
        public int? Rating { get; set; }
        public FeedbackServiceStatus? Status { get; set; }
        public string? Comment { get; set; }
    }
}
