namespace BookfetSystem.Services.Interface
{
    public interface IStaffTaskOverdueSchedulerService
    {
        Task ScheduleTaskOverdueCheckAsync(int taskId, DateTime? endTime);
    }
}
