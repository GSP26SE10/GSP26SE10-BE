using System.Collections.Generic;

namespace BookfetSystem.Services.Models
{
    public class RevenueItemResponse
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueSummaryResponse
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
    }

    public class RevenueChartResponse
    {
        public string GroupBy { get; set; }
        public List<RevenueItemResponse> Data { get; set; }
        public RevenueSummaryResponse Summary { get; set; }
    }
}