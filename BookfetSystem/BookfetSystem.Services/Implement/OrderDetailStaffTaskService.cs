using BookfetSystem.Repositories;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class OrderDetailStaffTaskService : IOrderDetailStaffTaskService
    {
        private readonly OrderDetailStaffTaskRepository _taskRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly UserRepository _userRepository;
        private readonly StaffGroupRepository _staffGroupRepository;
        private readonly StaffGroupMemberRepository _staffGroupMemberRepository;
        private readonly INotificationService _notificationService;

        public OrderDetailStaffTaskService(
            OrderDetailStaffTaskRepository taskRepository,
            OrderDetailRepository orderDetailRepository,
            UserRepository userRepository,
            StaffGroupRepository staffGroupRepository,
            StaffGroupMemberRepository staffGroupMemberRepository,
            INotificationService notificationService)
        {
            _taskRepository = taskRepository;
            _orderDetailRepository = orderDetailRepository;
            _userRepository = userRepository;
            _staffGroupRepository = staffGroupRepository;
            _staffGroupMemberRepository = staffGroupMemberRepository;
            _notificationService = notificationService;
        }

        public async Task<PagedResponse<StaffMyTaskResponse>> GetMyTasksAsync(int staffId, int page, int pageSize)
        {
            var query = _taskRepository.GetMyTasksByStaffId(staffId);
            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<StaffMyTaskResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<StaffMyTaskResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResponse<OrderDetailStaffTaskResponse>> GetAllOrderDetailStaffTaskFilteredAsync(OrderDetailStaffTaskFilterRequest request, int page, int pageSize)
        {
            var entityFilter = request.Adapt<OrderDetailStaffTask>();
            var query = _taskRepository.GetAllOrderDetailStaffTaskFiltered(entityFilter);

            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<OrderDetailStaffTaskResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<OrderDetailStaffTaskResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderDetailStaffTaskResponse>> CreateAsync(OrderDetailStaffTaskCreateRequest request, int leaderId)
        {
            // 1. Tìm staff group của leader
            var staffGroup = await _staffGroupRepository
                .GetAllStaffGroupFiltered(new StaffGroup { LeaderId = leaderId })
                .FirstOrDefaultAsync(sg => sg.Status == StaffGroupStatus.ACTIVE.ToString());

            if (staffGroup == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Leader does not have an active staff group.",
                    Data = null
                };
            }

            // 2. Kiểm tra order detail tồn tại và thuộc staff group của leader
            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            if (orderDetail.StaffGroupId != staffGroup.StaffGroupId)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Order detail does not belong to your staff group.",
                    Data = null
                };
            }

            // 3. Kiểm tra staff tồn tại
            var staff = await _userRepository.GetByIdAsync(request.StaffId);
            if (staff == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Staff not found.",
                    Data = null
                };
            }

            // 4. Kiểm tra staff có là thành viên của group leader không
            var isMember = await _staffGroupMemberRepository.ExistsAsync(staffGroup.StaffGroupId, request.StaffId);
            if (!isMember)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Staff does not belong to your staff group.",
                    Data = null
                };
            }
            var entity = new OrderDetailStaffTask
            {
                OrderDetailId = request.OrderDetailId,
                StaffId = request.StaffId,
                TaskName = request.TaskName?.Trim(),
                TaskStatus = StaffTaskStatus.PENDING.ToString(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Note = request.Note?.Trim()
            };

            var affected = await _taskRepository.CreateAsync(entity);
            if (affected > 0)
            {
                var response = new OrderDetailStaffTaskResponse
                {
                    TaskId = entity.TaskId,
                    OrderDetailId = entity.OrderDetailId,
                    StaffId = entity.StaffId,
                    TaskName = entity.TaskName,
                    TaskStatus = EnumHelper.TryParseToInt<StaffTaskStatus>(entity.TaskStatus),
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    Note = entity.Note,
                    StaffName = staff.FullName
                };

                await _notificationService.SendToUserAsync(
                    request.StaffId,
                    "Ban co cong viec moi",
                    $"Nhiem vu '{entity.TaskName ?? "Task"}' da duoc giao cho ban.",
                    NotificationType.Task,
                    new Dictionary<string, string>
                    {
                        ["taskId"] = entity.TaskId.ToString(),
                        ["orderDetailId"] = entity.OrderDetailId?.ToString() ?? string.Empty
                    });

                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = true,
                    Message = "Task created successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderDetailStaffTaskResponse>
            {
                Success = false,
                Message = "Failed to create task.",
                Data = null
            };
        }

        public async Task<ApiResponse<OrderDetailStaffTaskResponse>> UpdateAsync(int id, OrderDetailStaffTaskUpdateRequest request)
        {
            var entity = await _taskRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Task not found.",
                    Data = null
                };
            }

            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Order detail not found.",
                    Data = null
                };
            }

            var staff = await _userRepository.GetByIdAsync(request.StaffId);
            if (staff == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Staff not found.",
                    Data = null
                };
            }

            entity.OrderDetailId = request.OrderDetailId;
            entity.StaffId = request.StaffId;
            entity.TaskName = request.TaskName?.Trim();
            if (request.TaskStatus.HasValue)
            {
                entity.TaskStatus = request.TaskStatus.Value.ToString();
            }
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.Note = request.Note?.Trim();

            var affected = await _taskRepository.UpdateAsync(entity);
            if (affected > 0)
            {
                var response = new OrderDetailStaffTaskResponse
                {
                    TaskId = entity.TaskId,
                    OrderDetailId = entity.OrderDetailId,
                    StaffId = entity.StaffId,
                    TaskName = entity.TaskName,
                    TaskStatus = EnumHelper.TryParseToInt<StaffTaskStatus>(entity.TaskStatus),
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    Note = entity.Note,
                    StaffName = staff.FullName
                };

                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = true,
                    Message = "Task updated successfully.",
                    Data = response
                };
            }

            return new ApiResponse<OrderDetailStaffTaskResponse>
            {
                Success = false,
                Message = "Failed to update task.",
                Data = null
            };
        }

        public async Task<ApiResponse<OrderDetailStaffTaskResponse>> UpdateMyTaskStatusAsync(int taskId, int staffId, StaffUpdateTaskStatusRequest request)
        {
            var entity = await _taskRepository.GetByIdAsync(taskId);
            if (entity == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Task not found.",
                    Data = null
                };
            }

            if (entity.StaffId != staffId)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "You are not allowed to update this task.",
                    Data = null
                };
            }

            entity.TaskStatus = request.TaskStatus.ToString();
            var affected = await _taskRepository.UpdateAsync(entity);
            if (affected <= 0)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Failed to update task status.",
                    Data = null
                };
            }

            var staff = await _userRepository.GetByIdAsync(staffId);
            var response = new OrderDetailStaffTaskResponse
            {
                TaskId = entity.TaskId,
                OrderDetailId = entity.OrderDetailId,
                StaffId = entity.StaffId,
                TaskName = entity.TaskName,
                TaskStatus = EnumHelper.TryParseToInt<StaffTaskStatus>(entity.TaskStatus),
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Note = entity.Note,
                StaffName = staff?.FullName
            };

            if (request.TaskStatus == StaffTaskStatus.COMPLETED && entity.OrderDetailId.HasValue)
            {
                var orderDetail = await _orderDetailRepository.GetByIdAsync(entity.OrderDetailId.Value);
                if (orderDetail?.StaffGroupId.HasValue == true)
                {
                    var staffGroup = await _staffGroupRepository.GetByIdAsync(orderDetail.StaffGroupId.Value);
                    if (staffGroup?.LeaderId.HasValue == true)
                    {
                        await _notificationService.SendToUserAsync(
                            staffGroup.LeaderId.Value,
                            "Staff da hoan thanh cong viec",
                            $"{staff?.FullName ?? "Mot staff"} da hoan thanh nhiem vu '{entity.TaskName ?? "Task"}'.",
                            NotificationType.Task,
                            new Dictionary<string, string>
                            {
                                ["taskId"] = entity.TaskId.ToString(),
                                ["orderDetailId"] = entity.OrderDetailId.Value.ToString(),
                                ["staffId"] = staffId.ToString()
                            });
                    }
                }
            }

            return new ApiResponse<OrderDetailStaffTaskResponse>
            {
                Success = true,
                Message = "Task status updated successfully.",
                Data = response
            };
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _taskRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Task not found.",
                    Data = false
                };
            }

            var removed = await _taskRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Task deleted successfully.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to delete task.",
                Data = false
            };
        }
    }
}
