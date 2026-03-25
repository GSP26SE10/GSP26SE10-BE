using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class AuthenticationService : IAuthenticationService
    {
        private const string VerifyCodeCacheKeyPrefix = "verify:";
        private const string ResetPasswordCacheKeyPrefix = "reset:";
        private const string ChangePasswordCacheKeyPrefix = "change-password:";
        private const string DefaultVerifyCodeChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int DefaultVerifyCodeLength = 6;

        private readonly UserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ICache _cache;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            UserRepository userRepository,
            IEmailService emailService,
            ICache cache,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _cache = cache;
            _configuration = configuration;
        }

        /// <summary>
        /// Sinh mã xác thực từ bộ ký tự và độ dài cấu hình (Verification:VerifyCodeChars, Verification:VerifyCodeLength).
        /// </summary>
        public string GenerateVerifyCode()
        {
            var verification = _configuration.GetSection("Verification");
            var charsStr = verification["VerifyCodeChars"] ?? DefaultVerifyCodeChars;
            var length = int.TryParse(verification["VerifyCodeLength"], out var n) && n > 0 ? n : DefaultVerifyCodeLength;
            var chars = charsStr.ToCharArray();
            if (chars.Length == 0) chars = DefaultVerifyCodeChars.ToCharArray();

            var span = new char[length];
            var rnd = Random.Shared;
            for (int i = 0; i < length; i++)
                span[i] = chars[rnd.Next(chars.Length)];
            return new string(span);
        }
        public async Task<ApiResponse<LoginResponse>> Login(LoginRequest loginRequest)
        {
            var user = await _userRepository.GetUserByUsernameOrEmailAsync(loginRequest.UserNameOrEmail);
            if(user == null)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Email/Username or password is invalid",
                    Data = null
                };
            }
            if(user.Status != "ACTIVE")
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Your account is not active",
                    Data = null
                };
            }
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Email/Username or password is invalid",
                    Data = null
                };
            }
            var token = await GenerateToken(user);
            var loginResponse = new LoginResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Address = user.Address,
                Status = user.Status,
                UserName = user.UserName,
                Phone = user.Phone,
                RoleName = user.Role != null ? user.Role.RoleName : string.Empty,
                AccessToken = token
            };
            return new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = loginResponse
            };
        }
        public Task<string> GenerateToken(User user)
        {
            var jwtConfig = _configuration.GetSection("JwtConfig");

            var issuer = jwtConfig["Issuer"];
            var audience = jwtConfig["Audience"];
            var key = jwtConfig["Key"];
            var expiryIn = DateTime.Now.AddMinutes(Double.Parse(jwtConfig["ExpireMinutes"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.RoleId.ToString())
                }),
                Expires = expiryIn,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);
            return Task.FromResult(accessToken);
        }

        public async Task<ApiResponse<LoginResponse>> LoginGoogle(string code, string redirectUri)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return new ApiResponse<LoginResponse>()
                    {
                        Success = false,
                        Message = "Authorization code is required",
                        Data = null
                    };
                }

                var googleConfig = _configuration.GetSection("Google");
                var clientId = googleConfig["ClientId"];
                var clientSecret = googleConfig["ClientSecret"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    return new ApiResponse<LoginResponse>()
                    {
                        Success = false,
                        Message = "Google OAuth configuration is missing",
                        Data = null
                    };
                }

                // 1. Đổi authorization code lấy access token
                using var httpClient = new HttpClient();
                var tokenRequest = new
                {
                    code = code,
                    client_id = clientId,
                    client_secret = clientSecret,
                    redirect_uri = redirectUri,
                    grant_type = "authorization_code"
                };

                var tokenResponse = await httpClient.PostAsJsonAsync("https://oauth2.googleapis.com/token", tokenRequest);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<LoginResponse>()
                    {
                        Success = false,
                        Message = "Failed to exchange authorization code for token",
                        Data = null
                    };
                }

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
                var accessToken = tokenData.GetProperty("access_token").GetString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return new ApiResponse<LoginResponse>()
                    {
                        Success = false,
                        Message = "Failed to get access token from Google",
                        Data = null
                    };
                }

                // 2. Lấy thông tin user từ Google
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var userInfoResponse = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    return new ApiResponse<LoginResponse>()
                    {
                        Success = false,
                        Message = "Failed to get user information from Google",
                        Data = null
                    };
                }

                var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoContent);

                var email = userInfo.GetProperty("email").GetString();
                var fullName = userInfo.GetProperty("name").GetString();
                var picture = userInfo.TryGetProperty("picture", out var picElement) ? picElement.GetString() : null;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
                {
                    return new ApiResponse<LoginResponse>()
                    {
                        Success = false,
                        Message = "Failed to get email or name from Google",
                        Data = null
                    };
                }

                // 3. Tìm hoặc tạo user
                var account = await _userRepository.GetUserByEmailAsync(email);

                if (account == null)
                {
                    // Tạo user mới, dùng luôn email làm username
                    var randomPassword = Guid.NewGuid().ToString();

                    var newUser = new User
                    {
                        FullName = fullName,
                        Email = email,
                        Phone = null,
                        Status = "ACTIVE",
                        UserName = email,
                        Address = null,
                        RoleId = 4, // default USER role
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword),
                        CreatedAt = DateTime.UtcNow
                    };

                    var created = await _userRepository.CreateAsync(newUser);
                    if (created <= 0)
                    {
                        return new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = "Failed to create user from Google login",
                            Data = null
                        };
                    }

                    account = newUser;
                }

                // 4. Kiểm tra user status
                if (account.Status != "ACTIVE")
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Account is not active",
                        Data = null
                    };
                }

                // 5. Generate JWT token
                var jwtToken = await GenerateToken(account);

                var response = account.Adapt<LoginResponse>();
                response.AccessToken = jwtToken;
                // Map vẫn chạy khi DB null (chỉ ra null). Ghi đè bằng giá trị từ Google để response luôn có email + fullName.
                response.Email = email;
                response.FullName = fullName;

                return new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Google login successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>()
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> Register(RegisterRequest request)
        {
            var existingByEmail = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingByEmail != null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Email is already registered.",
                    Data = false
                };
            }

            var existingByUsername = await _userRepository.GetUserByUserName(request.UserName);
            if (existingByUsername != null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Username already exists.",
                    Data = false
                };
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.UserName,
                Phone = request.Phone,
                Address = request.Address,
                Status = "INACTIVE",
                RoleId = 4, // USER
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            var created = await _userRepository.CreateAsync(user);
            if (created <= 0)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to create account.",
                    Data = false
                };
            }

            var verifyCode = GenerateVerifyCode();
            var cacheKey = VerifyCodeCacheKeyPrefix + request.Email;
            _cache.Set(cacheKey, verifyCode, TimeSpan.FromMinutes(2));

            var subject = "Mã xác thực - Bookfet System";
            var htmlBody = BuildVerificationEmailBody(request.FullName, verifyCode);

            try
            {
                await _emailService.SendAsync(request.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Registration succeeded but sending email failed: {ex.Message}",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Registration successful. Please check your email for the verification code.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> VerifyEmail(VerifyEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Email and verification code are required.",
                    Data = false
                };
            }

            var cacheKey = VerifyCodeCacheKeyPrefix + request.Email.Trim();
            var cachedCode = _cache.Get(cacheKey);
            if (cachedCode == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Code has expired or does not exist. Please register again or request a new code.",
                    Data = false
                };
            }

            if (!string.Equals(cachedCode, request.Code.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid verification code.",
                    Data = false
                };
            }

            var user = await _userRepository.GetUserByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Account not found.",
                    Data = false
                };
            }

            if (user.Status == "ACTIVE")
            {
                _cache.Remove(cacheKey);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Account was already verified. You can log in.",
                    Data = true
                };
            }

            user.Status = "ACTIVE";
            await _userRepository.UpdateAsync(user);
            _cache.Remove(cacheKey);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Email verified successfully. You can now log in.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> ResendVerificationCode(ResendVerificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Email is required.",
                    Data = false
                };
            }

            var email = request.Email.Trim();
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Account not found.",
                    Data = false
                };
            }

            if (user.Status != "INACTIVE")
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Account already verified or not found.",
                    Data = false
                };
            }

            var verifyCode = GenerateVerifyCode();
            var cacheKey = VerifyCodeCacheKeyPrefix + email;
            _cache.Set(cacheKey, verifyCode, TimeSpan.FromMinutes(2));

            var subject = "Mã xác thực - Bookfet System";
            var htmlBody = BuildVerificationEmailBody(user.FullName ?? user.Email, verifyCode);

            try
            {
                await _emailService.SendAsync(email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to send email: {ex.Message}",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "A new verification code has been sent to your email.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> ForgotPassword(ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Email or username is required.",
                    Data = false
                };
            }

            var identifier = request.EmailOrUsername.Trim();
            var user = await _userRepository.GetUserByUsernameOrEmailAsync(identifier);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Account not found.",
                    Data = false
                };
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User does not have a valid email.",
                    Data = false
                };
            }

            var resetCode = GenerateVerifyCode();
            var cacheKey = ResetPasswordCacheKeyPrefix + user.Email.Trim();
            _cache.Set(cacheKey, resetCode, TimeSpan.FromMinutes(10));

            var subject = "Đặt lại mật khẩu - Bookfet System";
            var htmlBody = BuildResetPasswordEmailBody(user.FullName ?? user.Email, resetCode);

            try
            {
                await _emailService.SendAsync(user.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to send reset password email: {ex.Message}",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Reset password code has been sent to your email.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> ResetPassword(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmailOrUsername) ||
                string.IsNullOrWhiteSpace(request.Code) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Email/username, code and new password are required.",
                    Data = false
                };
            }

            var identifier = request.EmailOrUsername.Trim();
            var user = await _userRepository.GetUserByUsernameOrEmailAsync(identifier);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Account not found.",
                    Data = false
                };
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User does not have a valid email.",
                    Data = false
                };
            }

            var cacheKey = ResetPasswordCacheKeyPrefix + user.Email.Trim();
            var cachedCode = _cache.Get(cacheKey);
            if (cachedCode == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Code has expired or does not exist. Please request a new code.",
                    Data = false
                };
            }

            if (!string.Equals(cachedCode, request.Code.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid reset code.",
                    Data = false
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);
            _cache.Remove(cacheKey);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Password has been reset successfully.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> RequestChangePasswordOtp(int userId, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OldPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Old password, new password and confirm new password are required.",
                    Data = false
                };
            }

            if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Confirm new password does not match new password.",
                    Data = false
                };
            }

            if (request.NewPassword.Length < 6)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "New password must be at least 6 characters long.",
                    Data = false
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = false
                };
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User does not have a valid email.",
                    Data = false
                };
            }

            var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
            if (!isOldPasswordValid)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Old password is incorrect.",
                    Data = false
                };
            }

            var isSameAsOldPassword = BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash);
            if (isSameAsOldPassword)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "New password must be different from old password.",
                    Data = false
                };
            }

            var otpCode = GenerateVerifyCode();
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var cacheKey = ChangePasswordCacheKeyPrefix + user.Email.Trim();
            var payload = JsonSerializer.Serialize(new ChangePasswordOtpCacheData
            {
                Code = otpCode,
                NewPasswordHash = newPasswordHash
            });

            _cache.Set(cacheKey, payload, TimeSpan.FromMinutes(10));

            var subject = "Xác nhận đổi mật khẩu - Bookfet System";
            var htmlBody = BuildChangePasswordEmailBody(user.FullName ?? user.Email, otpCode);

            try
            {
                await _emailService.SendAsync(user.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to send change password OTP: {ex.Message}",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "OTP has been sent to your email.",
                Data = true
            };
        }

        public async Task<ApiResponse<bool>> VerifyChangePasswordOtp(int userId, VerifyChangePasswordOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "OTP code is required.",
                    Data = false
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = false
                };
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User does not have a valid email.",
                    Data = false
                };
            }

            var cacheKey = ChangePasswordCacheKeyPrefix + user.Email.Trim();
            var cachedPayload = _cache.Get(cacheKey);
            if (cachedPayload == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "OTP has expired or does not exist. Please request a new OTP.",
                    Data = false
                };
            }

            ChangePasswordOtpCacheData? payload;
            try
            {
                payload = JsonSerializer.Deserialize<ChangePasswordOtpCacheData>(cachedPayload);
            }
            catch
            {
                payload = null;
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.Code) || string.IsNullOrWhiteSpace(payload.NewPasswordHash))
            {
                _cache.Remove(cacheKey);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid OTP data. Please request a new OTP.",
                    Data = false
                };
            }

            if (!string.Equals(payload.Code, request.Code.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid OTP code.",
                    Data = false
                };
            }

            user.PasswordHash = payload.NewPasswordHash;
            await _userRepository.UpdateAsync(user);
            _cache.Remove(cacheKey);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Password changed successfully.",
                Data = true
            };
        }

        private static string BuildVerificationEmailBody(string fullName, string verifyCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <h2>Xin chào {fullName}!</h2>
    <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Bookfet System</strong> — nền tảng đặt tiệc và quản lý sự kiện tin cậy. Để hoàn tất đăng ký và bảo vệ tài khoản của bạn, vui lòng sử dụng mã xác thực bên dưới.</p>
    <p>Mã xác thực của bạn là:</p>
    <p style='font-size: 24px; font-weight: bold; letter-spacing: 4px; color: #2563eb;'>{verifyCode}</p>
    <p>Mã có hiệu lực trong <strong>2 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
    <hr/>
    <p style='font-size: 12px; color: #666;'>Bookfet System</p>
</body>
</html>";
        }

        private static string BuildResetPasswordEmailBody(string fullName, string resetCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <h2>Xin chào {fullName}!</h2>
    <p>Bạn vừa yêu cầu đặt lại mật khẩu tại <strong>Bookfet System</strong>. Vui lòng sử dụng mã bên dưới để tiếp tục quá trình đặt lại mật khẩu.</p>
    <p>Mã đặt lại mật khẩu của bạn là:</p>
    <p style='font-size: 24px; font-weight: bold; letter-spacing: 4px; color: #dc2626;'>{resetCode}</p>
    <p>Mã có hiệu lực trong <strong>10 phút</strong>. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
    <hr/>
    <p style='font-size: 12px; color: #666;'>Bookfet System</p>
</body>
</html>";
        }

        private static string BuildChangePasswordEmailBody(string fullName, string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <h2>Xin chào {fullName}!</h2>
    <p>Bạn vừa yêu cầu đổi mật khẩu tại <strong>Bookfet System</strong>.</p>
    <p>Vui lòng sử dụng mã OTP bên dưới để xác nhận:</p>
    <p style='font-size: 24px; font-weight: bold; letter-spacing: 4px; color: #dc2626;'>{otpCode}</p>
    <p>Mã có hiệu lực trong <strong>10 phút</strong>. Nếu bạn không thực hiện thao tác này, vui lòng đổi mật khẩu ngay và liên hệ hỗ trợ.</p>
    <hr/>
    <p style='font-size: 12px; color: #666;'>Bookfet System</p>
</body>
</html>";
        }

        private sealed class ChangePasswordOtpCacheData
        {
            public string Code { get; set; } = string.Empty;
            public string NewPasswordHash { get; set; } = string.Empty;
        }
    }
}
