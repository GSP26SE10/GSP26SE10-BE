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
    public class StaffGroupService : IStaffGroupService
    {
        private readonly StaffGroupRepository _staffGroupRepository;
        private readonly UserRepository _userRepository;

        public StaffGroupService(StaffGroupRepository staffGroupRepository, UserRepository userRepository)
        {
            _staffGroupRepository = staffGroupRepository;
            _userRepository = userRepository;
        }

        public async Task<PagedResponse<StaffGroupResponse>> GetAllStaffGroupFilteredAsync(StaffGroupFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<StaffGroup>();
            var query = _staffGroupRepository.GetAllStaffGroupFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<StaffGroupResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<StaffGroupResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<StaffGroupResponse>> CreateAsync(StaffGroupCreateRequest request)
        {
            var leader = await _userRepository.GetByIdAsync(request.LeaderId);
            if (leader == null)
            {
                return new ApiResponse<StaffGroupResponse>
                {
                    Success = false,
                    Message = "Leader not found.",
                    Data = null
                };
            }

            var alreadyHasGroup = await _staffGroupRepository.LeaderHasGroupAsync(request.LeaderId);
            if (alreadyHasGroup)
            {
                return new ApiResponse<StaffGroupResponse>
                {
                    Success = false,
                    Message = "Leader already has a staff group.",
                    Data = null
                };
            }

            var entity = new StaffGroup
            {
                StaffGroupName = request.StaffGroupName?.Trim(),
                LeaderId = request.LeaderId,
                Status = StaffGroupStatus.ACTIVE.ToString()
            };

            var affected = await _staffGroupRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<StaffGroupResponse>();
                response.LeaderName = leader.FullName;

                return new ApiResponse<StaffGroupResponse>
                {
                    Success = true,
                    Message = "Staff group created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<StaffGroupResponse>
            {
                Success = false,
                Message = "Failed to create staff group.",
                Data = null
            };
        }

        public async Task<ApiResponse<StaffGroupResponse>> UpdateAsync(int id, StaffGroupUpdateRequest request)
        {
            var entity = await _staffGroupRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<StaffGroupResponse>
                {
                    Success = false,
                    Message = "Staff group not found.",
                    Data = null
                };
            }

            var leaderHasGroup = await _staffGroupRepository.LeaderHasGroupAsync(request.LeaderId);
            if (leaderHasGroup)
            {
                return new ApiResponse<StaffGroupResponse>
                {
                    Success = false,
                    Message = "Leader already in staff group",
                    Data = null
                };
            }

            var leader = await _userRepository.GetByIdAsync(request.LeaderId);
            if (leader == null)
            {
                return new ApiResponse<StaffGroupResponse>
                {
                    Success = false,
                    Message = "Leader not found.",
                    Data = null
                };
            }

            entity.StaffGroupName = request.StaffGroupName?.Trim();
            entity.LeaderId = request.LeaderId;
            if (request.Status.HasValue)
            {
                entity.Status = request.Status.Value.ToString();
            }

            var affected = await _staffGroupRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = entity.Adapt<StaffGroupResponse>();
                response.LeaderName = leader.FullName;

                return new ApiResponse<StaffGroupResponse>
                {
                    Success = true,
                    Message = "Staff group updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<StaffGroupResponse>
            {
                Success = false,
                Message = "Failed to update staff group.",
                Data = null
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _staffGroupRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Staff group not found.",
                    Data = false
                };
            }

            if (await _staffGroupRepository.HasOrderDetailsAsync(id))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete staff group because it is being used in orders.",
                    Data = false
                };
            }

            var removed = await _staffGroupRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Staff group deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete staff group.",
                Data = false
            };
        }

        public async Task<StaffGroupAssignmentOverviewResponse?> GetAssignmentOverviewByLeaderAsync(int leaderId)
        {
            var staffGroup = await _staffGroupRepository.GetAssignmentOverviewByLeaderIdAsync(leaderId);
            if (staffGroup == null)
            {
                return null;
            }

            var response = staffGroup.Adapt<StaffGroupAssignmentOverviewResponse>();

            response.StaffGroup.Members = response.StaffGroup.Members
                .Where(m => m.StaffId.HasValue)
                .ToList();

            return response;
        }
    }
}

