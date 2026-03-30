using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Response;
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
        private readonly IOwnerDashboardService _ownerDashboardService;

        public OwnerController(IOwnerDashboardService ownerDashboardService)
        {
            _ownerDashboardService = ownerDashboardService;
        }

        [HttpGet("revenue-chart")]
        public async Task<ActionResult<ApiResponse<OwnerRevenueChartResponse>>> GetRevenueChart([FromQuery] string groupBy = "day")
        {
            var result = await _ownerDashboardService.GetOwnerRevenueChartAsync(groupBy);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("top-selling-menus")]
        public async Task<ActionResult<ApiResponse<OwnerTopSellingMenuResponse>>> GetTopSellingMenus([FromQuery] int top = 5)
        {
            var result = await _ownerDashboardService.GetTopSellingMenusAsync(top);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
