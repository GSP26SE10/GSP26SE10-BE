using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderServiceCreateRequest
    {
        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class OrderServiceUpdateRequest
    {
        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "ServiceId is required.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class OrderServiceFilterRequest
    {
        public int OrderServiceId { get; set; }
        public int? OrderDetailId { get; set; }
        public int? ServiceId { get; set; }
    }
}
