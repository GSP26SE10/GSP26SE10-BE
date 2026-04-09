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
using System;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class OrderDetailStaffTaskService : IOrderDetailStaffTaskService
    {
        private readonly OrderDetailStaffTaskRepository _taskRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly UserRepository _userRepository;
        private readonly TaskTemplateRepository _taskTemplateRepository;
        private readonly StaffGroupRepository _staffGroupRepository;
        private readonly StaffGroupMemberRepository _staffGroupMemberRepository;
        private readonly INotificationService _notificationService;
        private readonly IStaffTaskOverdueSchedulerService _staffTaskOverdueSchedulerService;

        public OrderDetailStaffTaskService(
            OrderDetailStaffTaskRepository taskRepository,
            OrderDetailRepository orderDetailRepository,
            UserRepository userRepository,
            TaskTemplateRepository taskTemplateRepository,
            StaffGroupRepository staffGroupRepository,
            StaffGroupMemberRepository staffGroupMemberRepository,
            INotificationService notificationService,
            IStaffTaskOverdueSchedulerService staffTaskOverdueSchedulerService)
        {
            _taskRepository = taskRepository;
            _orderDetailRepository = orderDetailRepository;
            _userRepository = userRepository;
            _taskTemplateRepository = taskTemplateRepository;
            _staffGroupRepository = staffGroupRepository;
            _staffGroupMemberRepository = staffGroupMemberRepository;
            _notificationService = notificationService;
            _staffTaskOverdueSchedulerService = staffTaskOverdueSchedulerService;
        }

        public async Task<PagedResponse<StaffMyTaskResponse>> GetMyTasksAsync(int staffId, int page, int pageSize)
        {
            await MarkOverdueTasksAndNotifyLeadersAsync();

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
            await MarkOverdueTasksAndNotifyLeadersAsync();

            var entityFilter = request.Adapt<OrderDetailStaffTask>();
            var query = _taskRepository.GetAllOrderDetailStaffTaskFiltered(entityFilter, request.TaskName);

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
                    Message = "Trưởng nhóm chưa có nhóm nhân sự đang hoạt động.",
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
                    Message = "Không tìm thấy chi tiết đơn tiệc.",
                    Data = null
                };
            }

            if (orderDetail.StaffGroupId != staffGroup.StaffGroupId)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Chi tiết đơn tiệc không thuộc nhóm của bạn.",
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
                    Message = "Không tìm thấy nhân viên.",
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
                    Message = "Nhân viên không thuộc nhóm của bạn.",
                    Data = null
                };
            }

            var taskName = NormalizeTaskName(request.TaskName) ?? "Công việc";

            var taskTemplate = await ResolveTemplateByTaskNameAsync(taskName)
                               ?? await ResolveDefaultActiveTemplateAsync();
            if (taskTemplate == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Không tìm thấy mẫu công việc đang hoạt động để gán cho công việc.",
                    Data = null
                };
            }

            var entity = new OrderDetailStaffTask
            {
                OrderDetailId = request.OrderDetailId,
                TaskTemplateId = taskTemplate.TaskTemplateId,
                TaskName = taskName,
                StaffId = request.StaffId,
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
                    TaskName = entity.TaskName ?? string.Empty,
                    TaskStatus = EnumHelper.TryParseToInt<StaffTaskStatus>(entity.TaskStatus),
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    Note = entity.Note ?? string.Empty,
                    StaffName = staff.FullName ?? string.Empty
                };

                await _notificationService.SendToUserAsync(
                    request.StaffId,
                    "Bạn có công việc mới",
                    $"Công việc '{entity.TaskName ?? "Công việc"}' đã được giao cho bạn.",
                    NotificationType.Task,
                    new Dictionary<string, string>
                    {
                        ["taskId"] = entity.TaskId.ToString(),
                        ["orderDetailId"] = entity.OrderDetailId?.ToString() ?? string.Empty
                    });

                await _staffTaskOverdueSchedulerService.ScheduleTaskOverdueCheckAsync(entity.TaskId, entity.EndTime);

                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = true,
                    Message = "Tạo công việc thành công.",
                    Data = response
                };
            }

            return new ApiResponse<OrderDetailStaffTaskResponse>
            {
                Success = false,
                Message = "Không thể tạo công việc.",
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
                    Message = "Không tìm thấy công việc.",
                    Data = null
                };
            }

            var orderDetail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId);
            if (orderDetail == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Không tìm thấy chi tiết đơn tiệc.",
                    Data = null
                };
            }

            var staff = await _userRepository.GetByIdAsync(request.StaffId);
            if (staff == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Không tìm thấy nhân viên.",
                    Data = null
                };
            }

            var taskName = NormalizeTaskName(request.TaskName)
                           ?? NormalizeTaskName(entity.TaskName)
                           ?? "Công việc";

            var taskTemplate = await ResolveTemplateByTaskNameAsync(taskName);

            entity.OrderDetailId = request.OrderDetailId;
            if (taskTemplate != null)
            {
                entity.TaskTemplateId = taskTemplate.TaskTemplateId;
            }
            entity.TaskName = taskName;
            entity.StaffId = request.StaffId;
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
                await _staffTaskOverdueSchedulerService.ScheduleTaskOverdueCheckAsync(entity.TaskId, entity.EndTime);

                var response = new OrderDetailStaffTaskResponse
                {
                    TaskId = entity.TaskId,
                    OrderDetailId = entity.OrderDetailId,
                    StaffId = entity.StaffId,
                    TaskName = entity.TaskName ?? string.Empty,
                    TaskStatus = EnumHelper.TryParseToInt<StaffTaskStatus>(entity.TaskStatus),
                    StartTime = entity.StartTime,
                    EndTime = entity.EndTime,
                    Note = entity.Note ?? string.Empty,
                    StaffName = staff.FullName ?? string.Empty
                };

                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = true,
                    Message = "Cập nhật công việc thành công.",
                    Data = response
                };
            }

            return new ApiResponse<OrderDetailStaffTaskResponse>
            {
                Success = false,
                Message = "Không thể cập nhật công việc.",
                Data = null
            };
        }

        public async Task<ApiResponse<OrderDetailStaffTaskResponse>> UpdateMyTaskStatusAsync(int taskId, int staffId, StaffUpdateTaskStatusRequest request)
        {
            await MarkOverdueTasksAndNotifyLeadersAsync();

            var entity = await _taskRepository.GetByIdAsync(taskId);
            if (entity == null)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Không tìm thấy công việc.",
                    Data = null
                };
            }

            if (entity.StaffId != staffId)
            {
                return new ApiResponse<OrderDetailStaffTaskResponse>
                {
                    Success = false,
                    Message = "Bạn không có quyền cập nhật công việc này.",
                    Data = null
                };
            }

            var previousStatus = entity.TaskStatus;
            var finalStatus = request.TaskStatus;

            if (IsTaskOverdue(entity) && request.TaskStatus != StaffTaskStatus.COMPLETED && request.TaskStatus != StaffTaskStatus.CANCELLED)
            {
                finalStatus = StaffTaskStatus.OVERDUE;
            }

            entity.TaskStatus = finalStatus.ToString();
            var hasStatusChanged = !string.Equals(previousStatus, entity.TaskStatus, StringComparison.OrdinalIgnoreCase);

            if (hasStatusChanged)
            {
                var affected = await _taskRepository.UpdateAsync(entity);
                if (affected <= 0)
                {
                    return new ApiResponse<OrderDetailStaffTaskResponse>
                    {
                        Success = false,
                        Message = "Không thể cập nhật trạng thái công việc.",
                        Data = null
                    };
                }
            }

            var staff = await _userRepository.GetByIdAsync(staffId);
            var staffDisplayName = staff?.FullName ?? "Một nhân viên";
            var taskName = GetTaskDisplayName(entity);

            var response = new OrderDetailStaffTaskResponse
            {
                TaskId = entity.TaskId,
                OrderDetailId = entity.OrderDetailId,
                StaffId = entity.StaffId,
                TaskName = taskName,
                TaskStatus = EnumHelper.TryParseToInt<StaffTaskStatus>(entity.TaskStatus),
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Note = entity.Note ?? string.Empty,
                StaffName = staff?.FullName ?? string.Empty
            };

            if (entity.OrderDetailId.HasValue)
            {
                var orderDetail = await _orderDetailRepository.GetByIdAsync(entity.OrderDetailId.Value);
                if (orderDetail?.StaffGroupId.HasValue == true)
                {
                    var staffGroup = await _staffGroupRepository.GetByIdAsync(orderDetail.StaffGroupId.Value);
                    if (staffGroup?.LeaderId.HasValue == true)
                    {
                        var notificationTitle = finalStatus switch
                        {
                            StaffTaskStatus.COMPLETED => $"{staffDisplayName} đã hoàn thành công việc",
                            StaffTaskStatus.IN_PROGRESS => $"{staffDisplayName} đang làm việc",
                            StaffTaskStatus.OVERDUE => $"{staffDisplayName} bị trễ deadline",
                            _ => $"{staffDisplayName} đã cập nhật trạng thái công việc"
                        };

                        var notificationBody = finalStatus switch
                        {
                            StaffTaskStatus.COMPLETED => $"{staffDisplayName} đã hoàn thành nhiệm vụ '{taskName}'.",
                            StaffTaskStatus.IN_PROGRESS => $"{staffDisplayName} đang thực hiện công việc '{taskName}'.",
                            StaffTaskStatus.OVERDUE => $"Công việc '{taskName}' đã trễ deadline. Trưởng nhóm vui lòng xem xét giao cho người khác.",
                            _ => $"{staffDisplayName} đã cập nhật trạng thái công việc '{taskName}'."
                        };

                        var shouldNotifyLeader =
                            hasStatusChanged ||
                            finalStatus == StaffTaskStatus.COMPLETED ||
                            finalStatus == StaffTaskStatus.IN_PROGRESS;

                        if (shouldNotifyLeader)
                        {
                            await _notificationService.SendToUserAsync(
                                staffGroup.LeaderId.Value,
                                notificationTitle,
                                notificationBody,
                                NotificationType.Task,
                                new Dictionary<string, string>
                                {
                                    ["taskId"] = entity.TaskId.ToString(),
                                    ["orderDetailId"] = entity.OrderDetailId.Value.ToString(),
                                    ["staffId"] = staffId.ToString(),
                                    ["taskStatus"] = finalStatus.ToString()
                                });
                        }
                    }
                }
            }

            return new ApiResponse<OrderDetailStaffTaskResponse>
            {
                Success = true,
                Message = "Cập nhật trạng thái công việc thành công.",
                Data = response
            };
        }

        private bool IsTaskOverdue(OrderDetailStaffTask task)
        {
            if (!task.EndTime.HasValue)
            {
                return false;
            }

            return task.EndTime.Value < DateTime.UtcNow;
        }

        private async Task MarkOverdueTasksAndNotifyLeadersAsync()
        {
            var overdueTasks = await _taskRepository
                .GetOverdueTaskCandidates(DateTime.UtcNow)
                .ToListAsync();

            foreach (var task in overdueTasks)
            {
                task.TaskStatus = StaffTaskStatus.OVERDUE.ToString();
                var affected = await _taskRepository.UpdateAsync(task);
                if (affected <= 0 || !task.OrderDetailId.HasValue)
                {
                    continue;
                }

                var orderDetail = task.OrderDetail ?? await _orderDetailRepository.GetByIdAsync(task.OrderDetailId.Value);
                if (orderDetail?.StaffGroupId.HasValue != true)
                {
                    continue;
                }

                var staffGroupId = orderDetail.StaffGroupId.GetValueOrDefault();
                var staffGroup = await _staffGroupRepository.GetByIdAsync(staffGroupId);
                if (staffGroup == null || !staffGroup.LeaderId.HasValue)
                {
                    continue;
                }

                var leaderId = staffGroup.LeaderId.Value;

                var staffDisplayName = task.Staff?.FullName ?? "Một nhân viên";
                var taskName = GetTaskDisplayName(task);

                await _notificationService.SendToUserAsync(
                    leaderId,
                    $"{staffDisplayName} bị trễ deadline",
                    $"Công việc '{taskName}' đã trễ deadline. Trưởng nhóm vui lòng xem xét giao cho người khác.",
                    NotificationType.Task,
                    new Dictionary<string, string>
                    {
                        ["taskId"] = task.TaskId.ToString(),
                        ["orderDetailId"] = task.OrderDetailId.Value.ToString(),
                        ["staffId"] = task.StaffId?.ToString() ?? string.Empty,
                        ["taskStatus"] = StaffTaskStatus.OVERDUE.ToString()
                    });
            }
        }

        private static string GetTaskDisplayName(OrderDetailStaffTask task)
        {
            return NormalizeTaskName(task.TaskName)
                ?? NormalizeTaskName(task.TaskTemplate?.TaskName)
                ?? "Công việc";
        }

        private static string? NormalizeTaskName(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private Task<TaskTemplate?> ResolveTemplateByTaskNameAsync(string taskName)
        {
            return _taskTemplateRepository
                .GetAllTaskTemplateFiltered(new TaskTemplate { TaskName = taskName })
                .Where(t => t.IsActive == true && t.TaskName != null && t.TaskName.ToLower() == taskName.ToLower())
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        private Task<TaskTemplate?> ResolveDefaultActiveTemplateAsync()
        {
            return _taskTemplateRepository
                .GetAllTaskTemplateFiltered(new TaskTemplate { IsActive = true })
                .Where(t => t.IsActive == true)
                .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var entity = await _taskRepository.GetByIdAsync(id);
            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Không tìm thấy công việc.",
                    Data = false
                };
            }

            var removed = await _taskRepository.RemoveAsync(entity);
            if (removed)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Xóa công việc thành công.",
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Không thể xóa công việc.",
                Data = false
            };
        }
    }
}
