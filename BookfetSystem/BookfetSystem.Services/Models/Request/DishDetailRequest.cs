using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class DishDetailCreateRequest
    {
        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }

        [Required(ErrorMessage = "IngredientId is required.")]
        public int IngredientId { get; set; }
    }

    public class DishDetailUpdateRequest
    {
        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }

        [Required(ErrorMessage = "IngredientId is required.")]
        public int IngredientId { get; set; }
    }

    public class DishDetailFilterRequest
    {
        public int DishDetailId { get; set; }
        public int? DishId { get; set; }
        public int? IngredientId { get; set; }
    }
}
