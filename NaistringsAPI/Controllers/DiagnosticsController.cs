using Microsoft.AspNetCore.Mvc;

namespace NaistringsAPI.Controllers;

[ApiController]
[Route("api/v1/diagnostics")]
public sealed class DiagnosticsController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { application = "NaistringsAPI", message = "API is responding.", utcTime = DateTimeOffset.UtcNow });
}
