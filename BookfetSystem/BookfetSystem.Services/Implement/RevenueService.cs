using BookfetSystem.Repositories;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class RevenueService : IRevenueService
    {
        private readonly PaymentRepository _paymentRepository;

        public RevenueService(PaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<RevenueChartResponse> GetOwnerRevenueChartAsync(string groupBy = "day")
        {
            var normalizedGroupBy = string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase)
                ? "month"
                : "day";

            var (start, end) = GetRange(normalizedGroupBy);
            var now = DateTime.UtcNow;

            var payments = await _paymentRepository.GetPaidPayments()
                .Where(p => p.PaidAt >= start && p.PaidAt <= end)
                .ToListAsync();

            var data = normalizedGroupBy == "day"
                ? BuildDailyData(payments, start)
                : BuildMonthlyData(payments, now.Year, now.Month);

            return new RevenueChartResponse
            {
                GroupBy = normalizedGroupBy,
                Data = data,
                Summary = new RevenueSummaryResponse
                {
                    TotalRevenue = payments.Sum(p => p.Amount ?? 0m),
                    TotalOrders = payments
                        .Where(p => p.OrderId != null)
                        .Select(p => p.OrderId ?? 0)
                        .Distinct()
                        .Count()
                }
            };
        }

        private static (DateTime start, DateTime end) GetRange(string groupBy)
        {
            var now = DateTime.UtcNow;

            if (groupBy == "day")
            {
                return (now.Date.AddDays(-6), now.Date.AddDays(1).AddTicks(-1));
            }

            return (
                new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, DateTimeKind.Utc)
            );
        }

        private static List<RevenueItemResponse> BuildDailyData(List<BookfetSystem.Repositories.Entities.Payment> payments, DateTime start)
        {
            var grouped = payments
                .Where(p => p.PaidAt.HasValue)
                .GroupBy(p => p.PaidAt!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount ?? 0m));

            return Enumerable.Range(0, 7)
                .Select(i => start.Date.AddDays(i))
                .Select(date => new RevenueItemResponse
                {
                    Label = date.ToString("yyyy-MM-dd"),
                    Revenue = grouped.TryGetValue(date, out var revenue) ? revenue : 0m
                })
                .ToList();
        }

        private static List<RevenueItemResponse> BuildMonthlyData(List<BookfetSystem.Repositories.Entities.Payment> payments, int year, int maxMonth)
        {
            var grouped = payments
                .Where(p => p.PaidAt.HasValue)
                .GroupBy(p => new { p.PaidAt!.Value.Year, p.PaidAt!.Value.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.Amount ?? 0m));

            return Enumerable.Range(1, maxMonth)
                .Select(month => new RevenueItemResponse
                {
                    Label = $"{year}-{month:00}",
                    Revenue = grouped.TryGetValue((year, month), out var revenue) ? revenue : 0m
                })
                .ToList();
        }
    }
}