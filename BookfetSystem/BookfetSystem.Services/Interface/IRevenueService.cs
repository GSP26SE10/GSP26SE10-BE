using BookfetSystem.Services.Models;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IRevenueService
    {
        Task<RevenueChartResponse> GetOwnerRevenueChartAsync(string groupBy = "day");
    }
}