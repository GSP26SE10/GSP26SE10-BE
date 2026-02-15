using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IAuthenticationService
    {
        Task<ApiResponse<LoginResponse>> Login(LoginRequest loginRequest);
    }
}
