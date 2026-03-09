using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderDetailCustomCreateRequest
    {
        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class OrderDetailCustomUpdateRequest
    {
        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public decimal? TotalAmount { get; set; }
    }

    public class OrderDetailCustomFilterRequest
    {
        public int OrderDetailCustomId { get; set; }
        public int? OrderDetailId { get; set; }
        public int? DishId { get; set; }
        public int? Quantity { get; set; }
    }
}
