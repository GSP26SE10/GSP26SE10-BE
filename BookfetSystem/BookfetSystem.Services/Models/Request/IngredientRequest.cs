using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BookfetSystem.Services.Models.Request
{
    public class IngredientCreateRequest
    {
        [Required(ErrorMessage = "IngredientName is required.")]
        public string? IngredientName { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImgFile { get; set; }
    }

    public class IngredientUpdateRequest
    {
        [Required(ErrorMessage = "IngredientName is required.")]
        public string? IngredientName { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImgFile { get; set; }
    }

    public class IngredientFilterRequest
    {
        public int IngredientId { get; set; }
        public string? IngredientName { get; set; }
        public string? Description { get; set; }
        public string? Img { get; set; }
    }
}
