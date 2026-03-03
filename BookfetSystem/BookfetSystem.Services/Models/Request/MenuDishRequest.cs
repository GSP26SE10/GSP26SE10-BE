using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class MenuDishCreateRequest
    {
        [Required(ErrorMessage = "MenuId is required.")]
        public int MenuId { get; set; }

        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }
    }

    public class MenuDishUpdateRequest
    {
        [Required(ErrorMessage = "MenuId is required.")]
        public int MenuId { get; set; }

        [Required(ErrorMessage = "DishId is required.")]
        public int DishId { get; set; }
    }

    public class MenuDishFilterRequest
    {
        public int MenuDishId { get; set; }
        public int? MenuId { get; set; }
        public int? DishId { get; set; }
    }
}
