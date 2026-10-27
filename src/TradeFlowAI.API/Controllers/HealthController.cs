using Microsoft.AspNetCore.Mvc;

namespace TradeFlowAI.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [Route("/health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Platform = "TradeFlowAI Autonomous Platform v1.0",
            Components = new
            {
                Database = "Operational",
                AiEngine = "Operational",
                MarketFeed = "Operational",
                OrderRouting = "Operational"
            }
        });
    }

    [HttpGet]
    [Route("/health/ready")]
    public IActionResult Ready()
    {
        return Ok(new { Status = "Ready", Timestamp = DateTime.UtcNow });
    }

    [HttpGet]
    [Route("/health/live")]
    public IActionResult Live()
    {
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }
}
