using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly IImageStorageService _imageStorageService;

        public UserService(UserRepository userRepository, IImageStorageService imageStorageService)
        {
            _userRepository = userRepository;
            _imageStorageService = imageStorageService;
        }

        public async Task<ApiResponse<UserResponse>> CreateAsync(UserCreateRequest request)
        {
            if (!await _userRepository.CheckRoleExistAsync(request.RoleId))
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Role not exist, try correct input role id.",
                    Data = null
                };
            }
            var existUserName = await _userRepository.GetUserByUsernameOrEmailAsync(request.UserName);
            if (existUserName != null)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Username is existed.",
                    Data = null
                };
            }
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Email is existed.",
                    Data = null
                };
            }
            var entity = request.Adapt<User>();
            entity.PasswordHash = HashPassword(request.Password);
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = UserStatus.ACTIVE.ToString();

            try
            {
                if (request.AvatarFile != null)
                {
                    entity.Avatar = await _imageStorageService.UploadImageAsync(
                        request.AvatarFile,
                        CloudinaryFolder.User);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Failed to upload avatar: {ex.Message}",
                    Data = null
                };
            }

            var affected = await _userRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var userWithRole = await _userRepository.GetUserWithRoleByIdAsync(entity.UserId);
                var response = userWithRole!.Adapt<UserResponse>();
                return new ApiResponse<UserResponse>
                {
                    Success = true,
                    Message = "User created successfully.",
                    Data = response
                };
            }
            else
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Failed to create user.",
                    Data = null
                };
            }

        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _userRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = false
                };
            }
            var affected = await _userRepository.RemoveAsync(entity);
            if (affected)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Account deleted successfully.",
                    Data = true
                };
            }
            else
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to delete  account.",
                    Data = false
                };
            }
        }

        public async Task<PagedResponse<UserResponse>> GetAllUserFilteredAsync(UserFilterRequest request, int page, int pageSize)
        {
            var entity = request.Adapt<User>();
            var query = _userRepository.GetAllUserFiltered(entity);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<UserResponse>()   // ⭐ map ngay trong SQL
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<UserResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<UserResponse>> UpdateAsync(int id, UserUpdateRequest request)
        {
            var entity = await _userRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "User not found.",
                    Data = null
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existEmail = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existEmail != null && existEmail.UserId != id)
                {
                    return new ApiResponse<UserResponse>
                    {
                        Success = false,
                        Message = "Email is existed.",
                        Data = null
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
                entity.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.Address))
                entity.Address = request.Address;

            if (!string.IsNullOrWhiteSpace(request.Email))
                entity.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Phone))
                entity.Phone = request.Phone;

            if (request.Status != null)
                entity.Status = request.Status.ToString();

            try
            {
                if (request.AvatarFile != null)
                {
                    entity.Avatar = await _imageStorageService.UploadImageAsync(
                        request.AvatarFile,
                        CloudinaryFolder.User,
                        id);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = $"Failed to upload avatar: {ex.Message}",
                    Data = null
                };
            }

            var affected = await _userRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var userWithRole = await _userRepository.GetUserWithRoleByIdAsync(entity.UserId);
                var response = userWithRole!.Adapt<UserResponse>();
                return new ApiResponse<UserResponse>
                {
                    Success = true,
                    Message = "User updated successfully.",
                    Data = response
                };
            }
            else
            {
                return new ApiResponse<UserResponse>
                {
                    Success = false,
                    Message = "Failed to update user.",
                    Data = null
                };
            }

        }
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
