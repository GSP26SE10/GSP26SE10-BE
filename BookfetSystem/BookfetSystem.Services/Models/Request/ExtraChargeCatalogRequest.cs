using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class ExtraChargeCatalogCreateRequest
    {
        [Required(ErrorMessage = "ChargeType is required.")]
        public string? ChargeType { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        public string? Unit { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "UnitPrice must be greater than or equal to 0.")]
        public decimal? UnitPrice { get; set; }

    }

    public class ExtraChargeCatalogUpdateRequest
    {
        [Required(ErrorMessage = "ChargeType is required.")]
        public string? ChargeType { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        public string? Unit { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "UnitPrice must be greater than or equal to 0.")]
        public decimal? UnitPrice { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ExtraChargeCatalogStatus), ErrorMessage = "Invalid status value. Use 0 (Inactive) or 1 (Active).")]
        public ExtraChargeCatalogStatus Status { get; set; }
    }

    public class ExtraChargeCatalogFilterRequest
    {
        public int ExtraChargeCatalogId { get; set; }
        public string? ChargeType { get; set; }
        public string? Title { get; set; }
        [EnumDataType(typeof(ExtraChargeCatalogStatus), ErrorMessage = "Invalid status value. Use 0 (Inactive) or 1 (Active).")]
        public ExtraChargeCatalogStatus? Status { get; set; }
    }
}
