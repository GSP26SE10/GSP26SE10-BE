using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class PartyCategoryMenuCreateRequest
    {
        [Required(ErrorMessage = "PartyCategoryId is required.")]
        public int PartyCategoryId { get; set; }

        [Required(ErrorMessage = "MenuId is required.")]
        public int MenuId { get; set; }
    }

    public class PartyCategoryMenuUpdateRequest
    {
        [Required(ErrorMessage = "PartyCategoryId is required.")]
        public int PartyCategoryId { get; set; }

        [Required(ErrorMessage = "MenuId is required.")]
        public int MenuId { get; set; }
    }

    public class PartyCategoryMenuFilterRequest
    {
        public int PartyCategoryMenuId { get; set; }
        public int? PartyCategoryId { get; set; }
        public int? MenuId { get; set; }
    }
}
