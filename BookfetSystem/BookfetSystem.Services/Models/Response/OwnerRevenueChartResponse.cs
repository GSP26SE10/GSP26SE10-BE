using System.Collections.Generic;

namespace BookfetSystem.Services.Models.Response
{
    public class OwnerRevenueItemResponse
    {
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class OwnerRevenueSummaryResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
    }

    public class OwnerRevenueOrderResponse
    {
        public int? OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? OrderStatus { get; set; }
    }

    public class OwnerRevenueChartResponse
    {
        public string GroupBy { get; set; } = "day";
        public List<OwnerRevenueItemResponse> Data { get; set; } = new();
        public OwnerRevenueSummaryResponse Summary { get; set; } = new();
        public List<OwnerRevenueOrderResponse> Orders { get; set; } = new();
    }
}