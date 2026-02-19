using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class StaffGroupMemberService : IStaffGroupMemberService
    {
        private readonly StaffGroupMemberRepository _staffGroupMemberRepository;
        private readonly StaffGroupRepository _staffGroupRepository;
        private readonly UserRepository _userRepository;

        public StaffGroupMemberService(
            StaffGroupMemberRepository staffGroupMemberRepository,
            StaffGroupRepository staffGroupRepository,
            UserRepository userRepository)
        {
            _staffGroupMemberRepository = staffGroupMemberRepository;
            _staffGroupRepository = staffGroupRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResponse<StaffGroupMemberResponse>> GetAllStaffGroupMemberFilteredAsync(StaffGroupMemberFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<StaffGroupMember>();
            var query = _staffGroupMemberRepository.GetAllStaffGroupMemberFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<StaffGroupMemberResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<StaffGroupMemberResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<StaffGroupMemberResponse>> CreateAsync(StaffGroupMemberCreateRequest request)
        {
            var staffGroup = await _staffGroupRepository.GetByIdAsync(request.StaffGroupId);
            if (staffGroup == null)
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "Staff group not found.",
                    Data = null
                };
            }

            var staff = await _userRepository.GetUserWithRoleByIdAsync(request.StaffId);
            if (staff == null)
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "Staff not found.",
                    Data = null
                };
            }

            if (!IsStaffRole(staff.Role?.RoleName))
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "User must have STAFF or GROUP_LEADER role to be added to staff group.",
                    Data = null
                };
            }

            if (await _staffGroupMemberRepository.ExistsAsync(request.StaffGroupId, request.StaffId))
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "Staff is already a member of this group.",
                    Data = null
                };
            }

            var entity = new StaffGroupMember
            {
                StaffGroupId = request.StaffGroupId,
                StaffId = request.StaffId,
                Status = StaffGroupStatus.ACTIVE.ToString()
            };

            var affected = await _staffGroupMemberRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = new StaffGroupMemberResponse
                {
                    StaffGroupMemberId = entity.StaffGroupMemberId,
                    StaffGroupId = entity.StaffGroupId,
                    StaffId = entity.StaffId,
                    Status = entity.Status,
                    StaffName = staff.FullName,
                    StaffGroupName = staffGroup.StaffGroupName
                };

                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = true,
                    Message = "Staff group member created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<StaffGroupMemberResponse>
            {
                Success = false,
                Message = "Failed to create staff group member.",
                Data = null
            };
        }

        public async Task<ApiResponse<StaffGroupMemberResponse>> UpdateAsync(int id, StaffGroupMemberUpdateRequest request)
        {
            var entity = await _staffGroupMemberRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "Staff group member not found.",
                    Data = null
                };
            }

            var staffGroup = await _staffGroupRepository.GetByIdAsync(request.StaffGroupId);
            if (staffGroup == null)
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "Staff group not found.",
                    Data = null
                };
            }

            var staff = await _userRepository.GetUserWithRoleByIdAsync(request.StaffId);
            if (staff == null)
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "Staff not found.",
                    Data = null
                };
            }

            if (!IsStaffRole(staff.Role?.RoleName))
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "User must have STAFF or GROUP_LEADER role to be added to staff group.",
                    Data = null
                };
            }

            if (await _staffGroupMemberRepository.ExistsAsync(request.StaffGroupId, request.StaffId, id))
            {
                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = false,
                    Message = "Staff is already a member of this group.",
                    Data = null
                };
            }

            entity.StaffGroupId = request.StaffGroupId;
            entity.StaffId = request.StaffId;
            if (request.Status.HasValue)
            {
                entity.Status = request.Status.Value.ToString();
            }

            var affected = await _staffGroupMemberRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = new StaffGroupMemberResponse
                {
                    StaffGroupMemberId = entity.StaffGroupMemberId,
                    StaffGroupId = entity.StaffGroupId,
                    StaffId = entity.StaffId,
                    Status = entity.Status,
                    StaffName = staff.FullName,
                    StaffGroupName = staffGroup.StaffGroupName
                };

                return new ApiResponse<StaffGroupMemberResponse>
                {
                    Success = true,
                    Message = "Staff group member updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<StaffGroupMemberResponse>
            {
                Success = false,
                Message = "Failed to update staff group member.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _staffGroupMemberRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Staff group member not found.",
                    Data = false
                };
            }

            var removed = await _staffGroupMemberRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Staff group member deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete staff group member.",
                Data = false
            };
        }

        private static bool IsStaffRole(string? roleName)
        {
            return roleName is "STAFF" or "GROUP_LEADER";
        }
    }
}
