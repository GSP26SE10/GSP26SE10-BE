using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Request
{
    public class ForgotPasswordRequest
    {
        public string EmailOrUsername { get; set; } = string.Empty;
    }
}

