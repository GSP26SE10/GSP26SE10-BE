using BookfetSystem.Repositories;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Implement
{
    public class OwnerDashboardService : IOwnerDashboardService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly OrderDetailRepository _orderDetailRepository;

        public OwnerDashboardService(PaymentRepository paymentRepository, OrderDetailRepository orderDetailRepository)
        {
            _paymentRepository = paymentRepository;
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<OwnerRevenueChartResponse> GetOwnerRevenueChartAsync(string groupBy = "day")
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

            return new OwnerRevenueChartResponse
            {
                GroupBy = normalizedGroupBy,
                Data = data,
                Summary = new OwnerRevenueSummaryResponse
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

        public async Task<ApiResponse<OwnerTopSellingMenuResponse>> GetTopSellingMenusAsync(int top = 5)
        {
            try
            {
                var normalizedTop = top <= 0 ? 5 : top;

                var paidOrderIds = _paymentRepository.GetPaidPayments()
                .Where(p => p.OrderId.HasValue)
                .Select(p => p.OrderId!.Value)
                .Distinct();

                var topItems = await _orderDetailRepository.GetAllOrderDetailFiltered(new BookfetSystem.Repositories.Entities.OrderDetail())
                    .Where(od => od.OrderId.HasValue && paidOrderIds.Contains(od.OrderId.Value) && od.MenuId.HasValue)
                    .GroupBy(od => od.MenuId!.Value)
                    .Select(g => new
                    {
                        MenuId = g.Key,
                        TotalOrders = g.Count(),
                        TotalGuests = g.Sum(x => x.NumberOfGuests ?? 0),
                        MenuName = g.Select(x => x.Menu != null ? x.Menu.MenuName : null).FirstOrDefault(),
                        ImgUrl = g.Select(x => x.Menu != null ? x.Menu.ImgUrl : null).FirstOrDefault(),
                        MenuSnapshot = g.Select(x => x.MenuSnapshot).FirstOrDefault()
                    })
                    .OrderByDescending(x => x.TotalOrders)
                    .ThenByDescending(x => x.TotalGuests)
                    .Take(normalizedTop)
                    .ToListAsync();

                var response = new OwnerTopSellingMenuResponse
                {
                    Items = topItems.Select(x =>
                    {
                        var snapshot = SnapshotParser.TryParseMenuSnapshot(x.MenuSnapshot);
                        return new OwnerTopSellingMenuItemResponse
                        {
                            MenuId = x.MenuId,
                            MenuName = !string.IsNullOrWhiteSpace(x.MenuName)
                                ? x.MenuName!
                                : (snapshot?.MenuName ?? string.Empty),
                            ImgUrl = !string.IsNullOrWhiteSpace(x.ImgUrl)
                                ? SnapshotParser.TryParseJsonToObject(x.ImgUrl)
                                : snapshot?.ImgUrl,
                            TotalOrders = x.TotalOrders,
                            TotalGuests = x.TotalGuests
                        };
                    }).ToList()
                };

                return new ApiResponse<OwnerTopSellingMenuResponse>
                {
                    Success = true,
                    Message = "Top selling menus loaded successfully.",
                    Data = response
                };
            }
            catch (Exception)
            {
                return new ApiResponse<OwnerTopSellingMenuResponse>
                {
                    Success = false,
                    Message = "Failed to load top selling menus.",
                    Data = null
                };
            }
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

        private static List<OwnerRevenueItemResponse> BuildDailyData(List<BookfetSystem.Repositories.Entities.Payment> payments, DateTime start)
        {
            var grouped = payments
                .Where(p => p.PaidAt.HasValue)
                .GroupBy(p => p.PaidAt!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount ?? 0m));

            return Enumerable.Range(0, 7)
                .Select(i => start.Date.AddDays(i))
                .Select(date => new OwnerRevenueItemResponse
                {
                    Label = date.ToString("yyyy-MM-dd"),
                    Revenue = grouped.TryGetValue(date, out var revenue) ? revenue : 0m
                })
                .ToList();
        }

            private static List<OwnerRevenueItemResponse> BuildMonthlyData(List<BookfetSystem.Repositories.Entities.Payment> payments, int year, int maxMonth)
        {
            var grouped = payments
                .Where(p => p.PaidAt.HasValue)
                .GroupBy(p => new { p.PaidAt!.Value.Year, p.PaidAt!.Value.Month })
                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.Amount ?? 0m));

            return Enumerable.Range(1, maxMonth)
                .Select(month => new OwnerRevenueItemResponse
                {
                    Label = $"{year}-{month:00}",
                    Revenue = grouped.TryGetValue((year, month), out var revenue) ? revenue : 0m
                })
                .ToList();
        }
    }
}