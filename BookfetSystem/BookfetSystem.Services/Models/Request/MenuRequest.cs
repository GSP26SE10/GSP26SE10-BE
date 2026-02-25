using System;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class MenuCreateRequest
    {
        [Required(ErrorMessage = "MenuName is required.")]
        public string? MenuName { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "BasePrice must be greater than or equal to 0.")]
        public decimal? BasePrice { get; set; }

        public string? ImgUrl { get; set; }

        public string? Status { get; set; }
    }

    public class MenuUpdateRequest
    {
        [Required(ErrorMessage = "MenuName is required.")]
        public string? MenuName { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "BasePrice must be greater than or equal to 0.")]
        public decimal? BasePrice { get; set; }

        public string? ImgUrl { get; set; }

        public string? Status { get; set; }
    }

    public class MenuFilterRequest
    {
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public decimal? MinBasePrice { get; set; }
        public decimal? MaxBasePrice { get; set; }
        public string? Status { get; set; }
    }
}