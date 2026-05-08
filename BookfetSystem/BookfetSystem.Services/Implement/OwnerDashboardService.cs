using BookfetSystem.Repositories;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

        public async Task<ApiResponse<OwnerRevenueChartResponse>> GetOwnerRevenueChartAsync(string groupBy = "day")
        {
            try
            {
                var normalizedGroupBy = string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase)
                    ? "month"
                    : "day";

                var (start, end) = GetRange(normalizedGroupBy);
                var now = DateTime.UtcNow;
                var completedStatus = OrderStatus.COMPLETED.ToString().ToLower();
                var cancelledStatus = OrderStatus.CANCELLED.ToString().ToLower();

                var payments = await _paymentRepository.GetPaidPayments()
                    .Where(p =>
                        p.PaidAt >= start &&
                        p.PaidAt <= end &&
                        p.Order != null &&
                        p.Order.Status != null &&
                        (
                            p.Order.Status.ToLower() == completedStatus ||
                            p.Order.Status.ToLower() == cancelledStatus
                        ))
                    .Include(p => p.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(od => od.OrderDetailExtraCharges)
                    .ToListAsync();

                var orderRevenues = BuildOrderRevenues(payments);

                var data = normalizedGroupBy == "day"
                    ? BuildDailyData(payments, start)
                    : BuildMonthlyData(payments, now.Year, now.Month);

                var response = new OwnerRevenueChartResponse
                {
                    GroupBy = normalizedGroupBy,
                    Data = data,
                    Orders = orderRevenues
                        .OrderByDescending(x => x.Amount)
                        .ToList(),
                    Summary = new OwnerRevenueSummaryResponse
                    {
                        TotalRevenue = orderRevenues.Sum(x => x.Amount),
                        TotalOrders = orderRevenues.Count
                    }
                };

                return new ApiResponse<OwnerRevenueChartResponse>
                {
                    Success = true,
                    Message = "Revenue chart loaded successfully.",
                    Data = response
                };
            }
            catch (Exception)
            {
                return new ApiResponse<OwnerRevenueChartResponse>
                {
                    Success = false,
                    Message = "Failed to load revenue chart.",
                    Data = null
                };
            }
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
                            TotalGuests = x.TotalGuests,
                            SoldQuantity = x.TotalGuests
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

        private static List<OwnerRevenueOrderResponse> BuildOrderRevenues(List<BookfetSystem.Repositories.Entities.Payment> payments)
        {
            var completedStatus = OrderStatus.COMPLETED.ToString();
            var cancelledStatus = OrderStatus.CANCELLED.ToString();

            return payments
                .Where(p => p.OrderId.HasValue)
                .GroupBy(p => p.OrderId!.Value)
                .Select(g =>
                {
                    var order = g.Select(x => x.Order).FirstOrDefault();
                    var orderStatus = order?.Status ?? string.Empty;
                    var latestPaidAt = g.Max(x => x.PaidAt);

                    decimal revenue;
                    DateTime? revenueAt = latestPaidAt;

                    if (string.Equals(orderStatus, completedStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        revenue = CalculateCompletedOrderRevenue(order);
                    }
                    else if (string.Equals(orderStatus, cancelledStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        revenue = GetRefundAmountFromOrderMetadata(order?.MtdZlp);
                        revenueAt = GetLatestRefundCreatedAt(order?.MtdZlp) ?? latestPaidAt;

                        if (revenue <= 0m)
                        {
                            revenue = g.Sum(x => x.Amount ?? 0m);
                        }
                    }
                    else
                    {
                        revenue = g.Sum(x => x.Amount ?? 0m);
                    }

                    return new OwnerRevenueOrderResponse
                    {
                        OrderId = g.Key,
                        Amount = revenue,
                        OrderStatus = orderStatus
                    };
                })
                .ToList();
        }

        private static decimal CalculateCompletedOrderRevenue(BookfetSystem.Repositories.Entities.Order? order)
        {
            if (order?.OrderDetails == null)
            {
                return 0m;
            }

            return order.OrderDetails.Sum(orderDetail =>
                (orderDetail.TotalPrice ?? 0m) +
                (orderDetail.OrderDetailExtraCharges?.Sum(extraCharge => extraCharge.TotalAmount ?? 0m) ?? 0m));
        }

        private static decimal GetRefundAmountFromOrderMetadata(string? rawMetadata)
        {
            if (string.IsNullOrWhiteSpace(rawMetadata))
            {
                return 0m;
            }

            try
            {
                using var json = JsonDocument.Parse(rawMetadata);
                if (!json.RootElement.TryGetProperty("Payments", out var paymentsElement) || paymentsElement.ValueKind != JsonValueKind.Array)
                {
                    return 0m;
                }

                decimal totalRefundAmount = 0m;

                foreach (var paymentElement in paymentsElement.EnumerateArray())
                {
                    if (!paymentElement.TryGetProperty("Refunds", out var refundsElement) || refundsElement.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var refundElement in refundsElement.EnumerateArray())
                    {
                        if (!refundElement.TryGetProperty("Amount", out var amountElement))
                        {
                            continue;
                        }

                        decimal parsedAmount;

                        if (amountElement.ValueKind == JsonValueKind.Number && amountElement.TryGetDecimal(out parsedAmount))
                        {
                            totalRefundAmount += parsedAmount;
                            continue;
                        }

                        if (amountElement.ValueKind == JsonValueKind.String &&
                            decimal.TryParse(amountElement.GetString(), out parsedAmount))
                        {
                            totalRefundAmount += parsedAmount;
                        }
                    }
                }

                return totalRefundAmount;
            }
            catch
            {
                return 0m;
            }
        }

        private static DateTime? GetLatestRefundCreatedAt(string? rawMetadata)
        {
            if (string.IsNullOrWhiteSpace(rawMetadata))
            {
                return null;
            }

            try
            {
                using var json = JsonDocument.Parse(rawMetadata);
                if (!json.RootElement.TryGetProperty("Payments", out var paymentsElement) || paymentsElement.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                DateTime? latestCreatedAt = null;

                foreach (var paymentElement in paymentsElement.EnumerateArray())
                {
                    if (!paymentElement.TryGetProperty("Refunds", out var refundsElement) || refundsElement.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var refundElement in refundsElement.EnumerateArray())
                    {
                        if (!refundElement.TryGetProperty("CreatedAt", out var createdAtElement))
                        {
                            continue;
                        }

                        DateTime createdAt;

                        if (createdAtElement.ValueKind == JsonValueKind.String && DateTime.TryParse(createdAtElement.GetString(), out createdAt))
                        {
                            if (!latestCreatedAt.HasValue || createdAt > latestCreatedAt.Value)
                            {
                                latestCreatedAt = createdAt;
                            }
                        }
                    }
                }

                return latestCreatedAt;
            }
            catch
            {
                return null;
            }
        }
    }
}