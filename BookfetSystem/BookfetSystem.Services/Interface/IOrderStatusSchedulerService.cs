namespace BookfetSystem.Services.Interface
{
    public interface IOrderStatusSchedulerService
    {
        Task ScheduleOrderDetailStatusTransitionsAsync(int orderDetailId, DateTime? startTime, DateTime? endTime);
        Task ScheduleOrderDepositTimeoutAsync(int orderId, DateTime? createdAt);
        Task SchedulePendingApprovalAutoCancelAsync(int orderId, DateTime? firstPartyStartTime);
    }
}
