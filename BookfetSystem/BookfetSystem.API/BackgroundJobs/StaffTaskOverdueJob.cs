using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.API.BackgroundJobs
{
    public interface IStaffTaskOverdueJob
    {
        Task MarkOverdueTasksAndNotifyLeaderAsync();
        Task MarkTaskOverdueIfDeadlinePassedAsync(int taskId, DateTime expectedEndTimeUtc);
    }

    public class StaffTaskOverdueJob : IStaffTaskOverdueJob
    {
        private readonly GSP26SE10DBContext _dbContext;
        private readonly INotificationService _notificationService;

        public StaffTaskOverdueJob(
            GSP26SE10DBContext dbContext,
            INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }

        public async Task MarkOverdueTasksAndNotifyLeaderAsync()
        {
            var utcNow = DateTime.UtcNow;
            var overdueTasks = await _dbContext.OrderDetailStaffTasks
                .Include(t => t.Staff)
                .Include(t => t.OrderDetail)
                .Where(t =>
                    t.EndTime.HasValue &&
                    t.EndTime.Value < utcNow &&
                    t.TaskStatus != StaffTaskStatus.COMPLETED.ToString() &&
                    t.TaskStatus != StaffTaskStatus.CANCELLED.ToString() &&
                    t.TaskStatus != StaffTaskStatus.OVERDUE.ToString())
                .ToListAsync();

            if (!overdueTasks.Any())
            {
                return;
            }

            foreach (var task in overdueTasks)
            {
                task.TaskStatus = StaffTaskStatus.OVERDUE.ToString();
            }

            await _dbContext.SaveChangesAsync();

            var staffGroupIds = overdueTasks
                .Where(t => t.OrderDetail?.StaffGroupId.HasValue == true)
                .Select(t => t.OrderDetail!.StaffGroupId!.Value)
                .Distinct()
                .ToList();

            var leaderMap = await _dbContext.StaffGroups
                .Where(sg => staffGroupIds.Contains(sg.StaffGroupId) && sg.LeaderId.HasValue)
                .ToDictionaryAsync(sg => sg.StaffGroupId, sg => sg.LeaderId!.Value);

            foreach (var task in overdueTasks)
            {
                var staffGroupId = task.OrderDetail?.StaffGroupId;
                if (!staffGroupId.HasValue || !leaderMap.TryGetValue(staffGroupId.Value, out var leaderId))
                {
                    continue;
                }

                await NotifyLeaderTaskOverdueAsync(task, leaderId);
            }
        }

        public async Task MarkTaskOverdueIfDeadlinePassedAsync(int taskId, DateTime expectedEndTimeUtc)
        {
            var task = await _dbContext.OrderDetailStaffTasks
                .Include(t => t.Staff)
                .Include(t => t.OrderDetail)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null || !task.EndTime.HasValue)
            {
                return;
            }

            var taskEndTimeUtc = ToUtc(task.EndTime.Value);
            if (Math.Abs((taskEndTimeUtc - expectedEndTimeUtc).TotalSeconds) > 5)
            {
                return;
            }

            if (string.Equals(task.TaskStatus, StaffTaskStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(task.TaskStatus, StaffTaskStatus.CANCELLED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(task.TaskStatus, StaffTaskStatus.OVERDUE.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (taskEndTimeUtc > DateTime.UtcNow)
            {
                return;
            }

            task.TaskStatus = StaffTaskStatus.OVERDUE.ToString();
            await _dbContext.SaveChangesAsync();

            var staffGroupId = task.OrderDetail?.StaffGroupId;
            if (!staffGroupId.HasValue)
            {
                return;
            }

            var leaderId = await _dbContext.StaffGroups
                .Where(sg => sg.StaffGroupId == staffGroupId.Value && sg.LeaderId.HasValue)
                .Select(sg => sg.LeaderId!.Value)
                .FirstOrDefaultAsync();

            if (leaderId <= 0)
            {
                return;
            }

            await NotifyLeaderTaskOverdueAsync(task, leaderId);
        }

        private async Task NotifyLeaderTaskOverdueAsync(BookfetSystem.Repositories.Entities.OrderDetailStaffTask task, int leaderId)
        {
            var staffDisplayName = string.IsNullOrWhiteSpace(task.Staff?.FullName) ? "Một nhân viên" : task.Staff!.FullName;
            var taskName = string.IsNullOrWhiteSpace(task.TaskName) ? "Công việc" : task.TaskName;

            try
            {
                await _notificationService.SendToUserAsync(
                    leaderId,
                    $"{staffDisplayName} bị trễ deadline",
                    $"Công việc '{taskName}' đã trễ deadline. Trưởng nhóm vui lòng xem xét giao việc cho người khác.",
                    NotificationType.Task,
                    new Dictionary<string, string>
                    {
                        ["taskId"] = task.TaskId.ToString(),
                        ["orderDetailId"] = task.OrderDetailId?.ToString() ?? string.Empty,
                        ["staffId"] = task.StaffId?.ToString() ?? string.Empty,
                        ["taskStatus"] = StaffTaskStatus.OVERDUE.ToString()
                    });
            }
            catch
            {
                // Notification failure should not stop overdue processing.
            }
        }

        private static DateTime ToUtc(DateTime input)
        {
            return input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };
        }
    }

    public class StaffTaskOverdueSchedulerService : IStaffTaskOverdueSchedulerService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public StaffTaskOverdueSchedulerService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Task ScheduleTaskOverdueCheckAsync(int taskId, DateTime? endTime)
        {
            if (taskId <= 0 || !endTime.HasValue)
            {
                return Task.CompletedTask;
            }

            var endTimeUtc = ToUtc(endTime.Value);
            if (endTimeUtc <= DateTime.UtcNow)
            {
                _backgroundJobClient.Enqueue<IStaffTaskOverdueJob>(
                    x => x.MarkTaskOverdueIfDeadlinePassedAsync(taskId, endTimeUtc));
            }
            else
            {
                _backgroundJobClient.Schedule<IStaffTaskOverdueJob>(
                    x => x.MarkTaskOverdueIfDeadlinePassedAsync(taskId, endTimeUtc),
                    endTimeUtc - DateTime.UtcNow);
            }

            return Task.CompletedTask;
        }

        private static DateTime ToUtc(DateTime input)
        {
            return input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };
        }
    }
}
