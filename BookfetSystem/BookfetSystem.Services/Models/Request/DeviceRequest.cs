using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class DeviceRegisterRequest
    {
        [Required(ErrorMessage = "UserId is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "ExpoPushToken is required.")]
        [MaxLength(255)]
        public string ExpoPushToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "DeviceId is required.")]
        [MaxLength(255)]
        public string DeviceId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Platform is required.")]
        [MaxLength(50)]
        public string Platform { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class DeviceDeactivateRequest
    {
        [Required(ErrorMessage = "DeviceId is required.")]
        [MaxLength(255)]
        public string DeviceId { get; set; } = string.Empty;
    }
}