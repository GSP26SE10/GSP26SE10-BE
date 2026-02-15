using BookfetSystem.Services.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Request
{
    public class UserCreateRequest
    {
        [Required(ErrorMessage = "UserName is required.")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "FullName is required.")]
        public string? FullName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string? Address { get; set; }
        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone must start with 0 and contain exactly 10 digits")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "RoleId is required.")]
        public int RoleId { get; set; }
    }

    public class UserUpdateRequest
    {
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "FullName is required.")]
        public string? FullName { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string? Address { get; set; }
        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone must start with 0 and contain exactly 10 digits")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "RoleId is required.")]
        public int RoleId { get; set; }
        [EnumDataType(typeof(UserStatus), ErrorMessage = "Invalid status value.")]
        public UserStatus? Status { get; set; }
    }

    public class UserFilterRequest
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public string? UserName { get; set; }
        public int? RoleId { get; set; }
    }
}
