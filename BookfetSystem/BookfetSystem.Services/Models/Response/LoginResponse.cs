using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Response
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string RoleName { get; set; }
        public string? AccessToken { get; set; }
    }
}
