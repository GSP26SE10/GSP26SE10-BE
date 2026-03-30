using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Response;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IOwnerDashboardService
    {
        Task<ApiResponse<OwnerRevenueChartResponse>> GetOwnerRevenueChartAsync(string groupBy = "day");
        Task<ApiResponse<OwnerTopSellingMenuResponse>> GetTopSellingMenusAsync(int top = 5);
    }
}