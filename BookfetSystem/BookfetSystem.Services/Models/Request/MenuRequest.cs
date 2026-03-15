using System;
using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;

namespace BookfetSystem.Services.Models.Request
{
    public class MenuCreateRequest
    {
        [Required(ErrorMessage = "MenuName is required.")]
        public string? MenuName { get; set; }

        [Required(ErrorMessage = "MenuCategoryId is required.")]
        public int MenuCategoryId { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "BasePrice must be greater than or equal to 0.")]
        public decimal? BasePrice { get; set; }

        public string? ImgUrl { get; set; }
    }

    public class MenuUpdateRequest
    {
        [Required(ErrorMessage = "MenuName is required.")]
        public string? MenuName { get; set; }

        [Required(ErrorMessage = "MenuCategoryId is required.")]
        public int MenuCategoryId { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "BasePrice must be greater than or equal to 0.")]
        public decimal? BasePrice { get; set; }

        public string? ImgUrl { get; set; }
        [EnumDataType(typeof(MenuStatus), ErrorMessage = "Invalid status value.")]
        public MenuStatus Status { get; set; }
    }

    public class MenuFilterRequest
    {
        public int MenuId { get; set; }
        public int? MenuCategoryId { get; set; }
        public string? MenuName { get; set; }
        public decimal? MinBasePrice { get; set; }
        public decimal? MaxBasePrice { get; set; }
        public MenuStatus? Status { get; set; }
    }
}