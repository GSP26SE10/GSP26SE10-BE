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
        Task<ApiResponse<LoginResponse>> LoginGoogle(string code, string redirectUri);
        Task<ApiResponse<bool>> Register(RegisterRequest request);
        Task<ApiResponse<bool>> VerifyEmail(VerifyEmailRequest request);
        Task<ApiResponse<bool>> ResendVerificationCode(ResendVerificationRequest request);
        Task<ApiResponse<bool>> ForgotPassword(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ResetPassword(ResetPasswordRequest request);
        Task<ApiResponse<bool>> RequestChangePasswordOtp(int userId, ChangePasswordRequest request);
        Task<ApiResponse<bool>> VerifyChangePasswordOtp(int userId, VerifyChangePasswordOtpRequest request);
    }
}
