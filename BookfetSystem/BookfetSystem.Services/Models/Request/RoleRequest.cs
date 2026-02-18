using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class RoleCreateRequest
    {
        [Required(ErrorMessage = "RoleName is required.")]
        [MaxLength(50, ErrorMessage = "RoleName must be at most 50 characters.")]
        public string? RoleName { get; set; }
    }

    public class RoleUpdateRequest
    {
        [Required(ErrorMessage = "RoleName is required.")]
        [MaxLength(50, ErrorMessage = "RoleName must be at most 50 characters.")]
        public string? RoleName { get; set; }
    }

    public class RoleFilterRequest
    {
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}

