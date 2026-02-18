using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class RoleService : IRoleService
    {
        private readonly RoleRepository _roleRepository;

        public RoleService(RoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<PagedResponse<RoleResponse>> GetAllRoleFilteredAsync(RoleFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<Role>();
            var query = _roleRepository.GetAllRoleFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<RoleResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<RoleResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<RoleResponse>> CreateAsync(RoleCreateRequest request)
        {
            var normalizedName = request.RoleName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<RoleResponse>
                {
                    Success = false,
                    Message = "RoleName is required.",
                    Data = null
                };
            }

            var exists = await _roleRepository
                .GetAllRoleFiltered(new Role { RoleName = normalizedName })
                .AnyAsync(r => r.RoleName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<RoleResponse>
                {
                    Success = false,
                    Message = "RoleName is existed.",
                    Data = null
                };
            }

            var entity = new Role
            {
                RoleName = normalizedName
            };

            var affected = await _roleRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<RoleResponse>();
                return new ApiResponse<RoleResponse>
                {
                    Success = true,
                    Message = "Role created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<RoleResponse>
            {
                Success = false,
                Message = "Failed to create role.",
                Data = null
            };
        }

        public async Task<ApiResponse<RoleResponse>> UpdateAsync(int id, RoleUpdateRequest request)
        {
            var entity = await _roleRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<RoleResponse>
                {
                    Success = false,
                    Message = "Role not found.",
                    Data = null
                };
            }

            var normalizedName = request.RoleName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return new ApiResponse<RoleResponse>
                {
                    Success = false,
                    Message = "RoleName is required.",
                    Data = null
                };
            }

            var exists = await _roleRepository
                .GetAllRoleFiltered(new Role { RoleName = normalizedName })
                .AnyAsync(r => r.RoleId != id && r.RoleName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return new ApiResponse<RoleResponse>
                {
                    Success = false,
                    Message = "RoleName is existed.",
                    Data = null
                };
            }

            entity.RoleName = normalizedName;

            var affected = await _roleRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<RoleResponse>();
                return new ApiResponse<RoleResponse>
                {
                    Success = true,
                    Message = "Role updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<RoleResponse>
            {
                Success = false,
                Message = "Failed to update role.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _roleRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Role not found.",
                    Data = false
                };
            }

            var removed = await _roleRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Role deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete role.",
                Data = false
            };
        }
    }
}

