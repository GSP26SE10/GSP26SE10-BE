namespace BookfetSystem.Services.Interface
{
    public interface IOrderStatusSchedulerService
    {
        Task ScheduleOrderDetailStatusTransitionsAsync(int orderDetailId, DateTime? startTime, DateTime? endTime);
    }
}
