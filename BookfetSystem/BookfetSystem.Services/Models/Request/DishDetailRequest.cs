using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class DishDetailCreateRequest
    {
        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }

        [Required(ErrorMessage = "IngredientId is required.")]
        public int IngredientId { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Quantity must be greater than or equal to 0.")]
        public decimal? Quantity { get; set; }

        public string? Unit { get; set; }
    }

    public class DishDetailUpdateRequest
    {
        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }

        [Required(ErrorMessage = "IngredientId is required.")]
        public int IngredientId { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Quantity must be greater than or equal to 0.")]
        public decimal? Quantity { get; set; }

        public string? Unit { get; set; }
    }

    public class DishDetailFilterRequest
    {
        public int DishDetailId { get; set; }
        public int? DishId { get; set; }
        public int? IngredientId { get; set; }
        public string? Unit { get; set; }
    }
}
