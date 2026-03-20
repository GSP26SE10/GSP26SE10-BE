using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Interface;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.API.BackgroundJobs
{
    public interface IOrderStatusTransitionJob
    {
        Task MoveToInProgressAsync(int orderDetailId, DateTime expectedStartTimeUtc);
        Task MoveToCompletedAsync(int orderDetailId, DateTime expectedEndTimeUtc);
    }

    public class OrderStatusTransitionJob : IOrderStatusTransitionJob
    {
        private readonly GSP26SE10DBContext _dbContext;

        public OrderStatusTransitionJob(GSP26SE10DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task MoveToInProgressAsync(int orderDetailId, DateTime expectedStartTimeUtc)
        {
            var detail = await _dbContext.OrderDetails
                .FirstOrDefaultAsync(x => x.OrderDetailId == orderDetailId);
            if (detail == null || !detail.StartTime.HasValue)
            {
                return;
            }

            var actualStartUtc = ToUtc(detail.StartTime.Value);

            if (string.Equals(detail.Status, OrderStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderStatus.CANCELLED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderStatus.REJECTED.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(detail.Status, OrderStatus.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            detail.Status = OrderStatus.IN_PROGRESS.ToString();
            await _dbContext.SaveChangesAsync();

            await RecalculateParentOrderStatusAsync(detail.OrderId);
        }

        public async Task MoveToCompletedAsync(int orderDetailId, DateTime expectedEndTimeUtc)
        {
            var detail = await _dbContext.OrderDetails
                .FirstOrDefaultAsync(x => x.OrderDetailId == orderDetailId);
            if (detail == null || !detail.EndTime.HasValue)
            {
                return;
            }

            var actualEndUtc = ToUtc(detail.EndTime.Value);

            if (string.Equals(detail.Status, OrderStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderStatus.CANCELLED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderStatus.REJECTED.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            detail.Status = OrderStatus.COMPLETED.ToString();
            await _dbContext.SaveChangesAsync();

            await RecalculateParentOrderStatusAsync(detail.OrderId);
        }

        private async Task RecalculateParentOrderStatusAsync(int? orderId)
        {
            if (!orderId.HasValue)
            {
                return;
            }

            var order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.OrderId == orderId.Value);

            if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return;
            }

            var detailStatuses = order.OrderDetails
                .Where(x => !string.IsNullOrWhiteSpace(x.Status))
                .Select(x => x.Status!)
                .ToList();

            if (detailStatuses.Count == 0)
            {
                return;
            }

            var allCompleted = detailStatuses.All(x => string.Equals(x, OrderStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase));
            if (allCompleted)
            {
                if (!string.Equals(order.Status, OrderStatus.BILLING.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    order.Status = OrderStatus.BILLING.ToString();
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                if (!string.Equals(order.Status, OrderStatus.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    order.Status = OrderStatus.IN_PROGRESS.ToString();
                    await _dbContext.SaveChangesAsync();
                }
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

    public class OrderStatusSchedulerService : IOrderStatusSchedulerService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public OrderStatusSchedulerService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Task ScheduleOrderDetailStatusTransitionsAsync(int orderDetailId, DateTime? startTime, DateTime? endTime)
        {
            if (startTime.HasValue)
            {
                var startUtc = ToUtc(startTime.Value);
                if (startUtc <= DateTime.UtcNow)
                {
                    _backgroundJobClient.Enqueue<IOrderStatusTransitionJob>(x => x.MoveToInProgressAsync(orderDetailId, startUtc));
                }
                else
                {
                    _backgroundJobClient.Schedule<IOrderStatusTransitionJob>(
                        x => x.MoveToInProgressAsync(orderDetailId, startUtc),
                        startUtc - DateTime.UtcNow);
                }
            }

            if (endTime.HasValue)
            {
                var endUtc = ToUtc(endTime.Value);
                if (endUtc <= DateTime.UtcNow)
                {
                    _backgroundJobClient.Enqueue<IOrderStatusTransitionJob>(x => x.MoveToCompletedAsync(orderDetailId, endUtc));
                }
                else
                {
                    _backgroundJobClient.Schedule<IOrderStatusTransitionJob>(
                        x => x.MoveToCompletedAsync(orderDetailId, endUtc),
                        endUtc - DateTime.UtcNow);
                }
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
