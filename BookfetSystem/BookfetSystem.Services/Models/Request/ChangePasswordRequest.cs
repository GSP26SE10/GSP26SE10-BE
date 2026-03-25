using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Old password is required.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm new password is required.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class VerifyChangePasswordOtpRequest
    {
        [Required(ErrorMessage = "OTP code is required.")]
        public string Code { get; set; } = string.Empty;
    }
}
