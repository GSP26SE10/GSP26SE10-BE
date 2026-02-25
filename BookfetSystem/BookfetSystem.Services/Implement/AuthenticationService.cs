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
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;
        public AuthenticationService(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
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
    }
}
