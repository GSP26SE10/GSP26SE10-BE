using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class DishCategoryCreateRequest
    {
        [Required(ErrorMessage = "DishCategoryName is required.")]
        public string? DishCategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class DishCategoryUpdateRequest
    {
        [Required(ErrorMessage = "DishCategoryName is required.")]
        public string? DishCategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class DishCategoryFilterRequest
    {
        public int DishCategoryId { get; set; }
        public string? DishCategoryName { get; set; }
        public string? Description { get; set; }
    }
}
