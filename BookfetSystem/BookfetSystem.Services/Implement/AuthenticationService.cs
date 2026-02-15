using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
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
    }
}
