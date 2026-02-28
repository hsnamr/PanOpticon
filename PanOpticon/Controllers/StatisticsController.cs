using Microsoft.AspNetCore.Mvc;
using PanOpticon.Constants;
using PanOpticon.Models;
using PanOpticon.Services;

namespace PanOpticon.Controllers;

[ApiController]
[Route("statistics")]
public class StatisticsController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly JobOrchestrationService _jobOrchestration;

    public StatisticsController(AuthService auth, JobOrchestrationService jobOrchestration)
    {
        _auth = auth;
        _jobOrchestration = jobOrchestration;
    }

    private UserContext? GetUser()
    {
        var authToken = Request.Headers["auth-token"].FirstOrDefault();
        var authorization = Request.Headers["Authorization"].FirstOrDefault();
        return _auth.GetCurrentUser(authToken, authorization);
    }

    [HttpPost("{pageName}/create")]
    public async Task<IActionResult> CreateJob(string pageName, [FromBody] CreateJobRequest request)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        if (!Pages.WidgetsByPage.ContainsKey(pageName))
            return NotFound(new { detail = "Page not found" });

        try
        {
            var result = await _jobOrchestration.CreateStatisticsJobAsync(pageName, request, user);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    [HttpGet("{pageName}/index")]
    public async Task<IActionResult> PollJob(string pageName, [FromQuery] string jobId)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        try
        {
            var result = await _jobOrchestration.PollJobAsync(jobId, pageName, user.CompanyId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { detail = "JOB_ID_NOT_FOUND" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }
}
