using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class MenuCategoryCreateRequest
    {
        [Required(ErrorMessage = "MenuCategoryName is required.")]
        public string? MenuCategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class MenuCategoryUpdateRequest
    {
        [Required(ErrorMessage = "MenuCategoryName is required.")]
        public string? MenuCategoryName { get; set; }
        public string? Description { get; set; }
        [EnumDataType(typeof(MenuStatus), ErrorMessage = "Invalid status value.")]
        public MenuStatus Status { get; set; }
    }

    public class MenuCategoryFilterRequest
    {
        public int MenuCategoryId { get; set; }
        public string? MenuCategoryName { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}
