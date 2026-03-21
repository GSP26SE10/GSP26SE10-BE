using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/owner")]
    [ApiController]
    [Authorize]
    public class OwnerController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public OwnerController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        [HttpGet("revenue-chart")]
        public async Task<ActionResult<RevenueChartResponse>> GetRevenueChart([FromQuery] string groupBy = "day")
        {
            var result = await _revenueService.GetOwnerRevenueChartAsync(groupBy);
            return Ok(result);
        }
    }
}
