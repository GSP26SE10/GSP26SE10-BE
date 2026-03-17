using System.ComponentModel.DataAnnotations;
using BookfetSystem.Services.Enum;
using Microsoft.AspNetCore.Http;

namespace BookfetSystem.Services.Models.Request
{
    public class DishCreateRequest
    {
        [Required(ErrorMessage = "DishName is required.")]
        public string? DishName { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Price must be greater than or equal to 0.")]
        public decimal? Price { get; set; }

        public string? Note { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImgFile { get; set; }
        public int? DishCategoryId { get; set; }
    }

    public class DishUpdateRequest
    {
        [Required(ErrorMessage = "DishName is required.")]
        public string? DishName { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Price must be greater than or equal to 0.")]
        public decimal? Price { get; set; }

        public string? Note { get; set; }
        public string? Description { get; set; }
        public DishStatus? Status { get; set; }
        public IFormFile? ImgFile { get; set; }
        public int? DishCategoryId { get; set; }
    }

    public class DishFilterRequest
    {
        public int DishId { get; set; }
        public string? DishName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DishStatus? Status { get; set; }
        public int? DishCategoryId { get; set; }
    }
}
