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

    public interface IOrderDepositTimeoutJob
    {
        Task CancelOrderIfDepositUnpaidAsync(int orderId, DateTime expectedCreatedAtUtc);
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

            if (string.Equals(detail.Status, OrderDetailStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.CANCELLED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.REJECTED.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(detail.Status, OrderDetailStatus.IN_PROGRESS.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            detail.Status = OrderDetailStatus.IN_PROGRESS.ToString();
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

            if (string.Equals(detail.Status, OrderDetailStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.CANCELLED.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail.Status, OrderDetailStatus.REJECTED.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            detail.Status = OrderDetailStatus.COMPLETED.ToString();
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

            var allCompleted = detailStatuses.All(x => string.Equals(x, OrderDetailStatus.COMPLETED.ToString(), StringComparison.OrdinalIgnoreCase));
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

    public class OrderDepositTimeoutJob : IOrderDepositTimeoutJob
    {
        private readonly GSP26SE10DBContext _dbContext;

        public OrderDepositTimeoutJob(GSP26SE10DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CancelOrderIfDepositUnpaidAsync(int orderId, DateTime expectedCreatedAtUtc)
        {
            var order = await _dbContext.Orders
                .Include(x => x.OrderDetails)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null || !order.CreatedAt.HasValue)
            {
                return;
            }

            var orderCreatedAtUtc = ToUtc(order.CreatedAt.Value);
            // Ignore stale jobs that don't match the actual order creation timestamp.
            if (Math.Abs((orderCreatedAtUtc - expectedCreatedAtUtc).TotalSeconds) > 5)
            {
                return;
            }

            if (!string.Equals(order.Status, OrderStatus.PENDING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var hasPaidDeposit = order.Payments.Any(p =>
                string.Equals(p.PaymentType, PaymentType.DEPOSIT.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.PaymentStatus, PaymentStatus.PAID.ToString(), StringComparison.OrdinalIgnoreCase));

            if (hasPaidDeposit || (order.DepositAmount ?? 0) > 0)
            {
                return;
            }

            // Final check right before cancellation to reduce webhook race conditions.
            var hasPaidDepositFinal = await _dbContext.Payments.AnyAsync(p =>
                p.OrderId == orderId &&
                p.PaymentType == PaymentType.DEPOSIT.ToString() &&
                p.PaymentStatus == PaymentStatus.PAID.ToString());

            if (hasPaidDepositFinal)
            {
                return;
            }

            order.Status = OrderStatus.CANCELLED.ToString();
            foreach (var detail in order.OrderDetails)
            {
                detail.Status = OrderDetailStatus.CANCELLED.ToString();
            }

            await _dbContext.SaveChangesAsync();
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

        public Task ScheduleOrderDepositTimeoutAsync(int orderId, DateTime? createdAt)
        {
            if (orderId <= 0 || !createdAt.HasValue)
            {
                return Task.CompletedTask;
            }

            var createdAtUtc = ToUtc(createdAt.Value);
            var executeAtUtc = createdAtUtc.AddMinutes(5);

            if (executeAtUtc <= DateTime.UtcNow)
            {
                _backgroundJobClient.Enqueue<IOrderDepositTimeoutJob>(
                    x => x.CancelOrderIfDepositUnpaidAsync(orderId, createdAtUtc));
            }
            else
            {
                _backgroundJobClient.Schedule<IOrderDepositTimeoutJob>(
                    x => x.CancelOrderIfDepositUnpaidAsync(orderId, createdAtUtc),
                    executeAtUtc - DateTime.UtcNow);
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
