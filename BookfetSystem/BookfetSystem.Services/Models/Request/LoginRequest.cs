using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username or email is required.")]
        public string? UserNameOrEmail { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }
    }
}
