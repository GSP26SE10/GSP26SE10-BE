using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class ServiceExtraChargeCatalogCreateRequest
    {
        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "ExtraChargeCatalogId is required.")]
        public int ExtraChargeCatalogId { get; set; }
    }

    public class ServiceExtraChargeCatalogUpdateRequest
    {
        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "ExtraChargeCatalogId is required.")]
        public int ExtraChargeCatalogId { get; set; }
    }

    public class ServiceExtraChargeCatalogFilterRequest
    {
        public int ServiceExtraChargeCatalogId { get; set; }
        public int? ServiceId { get; set; }
        public int? ExtraChargeCatalogId { get; set; }
    }
}
