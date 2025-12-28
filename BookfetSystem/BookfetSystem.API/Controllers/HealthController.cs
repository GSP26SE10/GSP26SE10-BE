using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("check-health")]
        public IActionResult CheckHealth()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
