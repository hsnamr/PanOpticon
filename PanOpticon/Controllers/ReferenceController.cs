using Microsoft.AspNetCore.Mvc;
using PanOpticon.Models;
using PanOpticon.Services;

namespace PanOpticon.Controllers;

[ApiController]
[Route("statistics")]
public class ReferenceController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly ReferenceDataService _referenceData;

    public ReferenceController(AuthService auth, ReferenceDataService referenceData)
    {
        _auth = auth;
        _referenceData = referenceData;
    }

    private UserContext? GetUser()
    {
        var authToken = Request.Headers["auth-token"].FirstOrDefault();
        var authorization = Request.Headers["Authorization"].FirstOrDefault();
        return _auth.GetCurrentUser(authToken, authorization);
    }

    [HttpGet("agents")]
    public IActionResult ListAgents()
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        var agents = _referenceData.GetAgents(user.CompanyId);
        return Ok(new { data = agents });
    }

    [HttpGet("teams")]
    public IActionResult ListTeams()
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        var teams = _referenceData.GetTeams(user.CompanyId);
        return Ok(new { data = teams });
    }

    [HttpGet("dataSources")]
    public IActionResult ListDataSources()
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        var sources = _referenceData.GetDataSources(user.CompanyId);
        return Ok(new { data = sources });
    }

    [HttpGet("promises")]
    public IActionResult ListSlas()
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        var promises = _referenceData.GetSlas(user.CompanyId);
        return Ok(new { data = promises });
    }

    [HttpGet("routings")]
    public IActionResult ListRoutings()
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        var routings = _referenceData.GetRoutings(user.CompanyId);
        return Ok(new { data = routings });
    }
}
