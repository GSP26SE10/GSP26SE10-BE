using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class GuestDiscountTierCreateRequest
    {
        [Required(ErrorMessage = "MinGuestCount is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "MinGuestCount must be greater than 0.")]
        public int MinGuestCount { get; set; }

        [Required(ErrorMessage = "DiscountPercent is required.")]
        [Range(typeof(decimal), "0", "100", ErrorMessage = "DiscountPercent must be between 0 and 100.")]
        public decimal DiscountPercent { get; set; }

        public string? Note { get; set; }
    }

    public class GuestDiscountTierUpdateRequest
    {
        [Required(ErrorMessage = "MinGuestCount is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "MinGuestCount must be greater than 0.")]
        public int MinGuestCount { get; set; }

        [Required(ErrorMessage = "DiscountPercent is required.")]
        [Range(typeof(decimal), "0", "100", ErrorMessage = "DiscountPercent must be between 0 and 100.")]
        public decimal DiscountPercent { get; set; }

        public string? Note { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(GuestDiscountTierStatus), ErrorMessage = "Invalid status value. Use 0 (Inactive) or 1 (Active).")]
        public GuestDiscountTierStatus Status { get; set; }
    }

    public class GuestDiscountTierFilterRequest
    {
        public int GuestDiscountTierId { get; set; }

        public int? MinGuestCount { get; set; }

        public decimal? DiscountPercent { get; set; }

        [EnumDataType(typeof(GuestDiscountTierStatus), ErrorMessage = "Invalid status value. Use 0 (Inactive) or 1 (Active).")]
        public GuestDiscountTierStatus? Status { get; set; }
    }
}
