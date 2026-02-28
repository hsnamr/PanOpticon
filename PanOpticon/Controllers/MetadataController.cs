using Microsoft.AspNetCore.Mvc;
using PanOpticon.Constants;
using PanOpticon.Models;
using PanOpticon.Services;

namespace PanOpticon.Controllers;

[ApiController]
[Route("statistics")]
public class MetadataController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly ReferenceDataService _referenceData;

    public MetadataController(AuthService auth, ReferenceDataService referenceData)
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

    [HttpGet("pages")]
    public IActionResult ListPages()
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        var data = Pages.All.Select(p => new PageItem(p.Id, p.Name)).ToList();
        return Ok(new PagesResponse(data));
    }

    [HttpGet("{pageName}/widgets")]
    public IActionResult GetWidgets(string pageName)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        if (!Pages.WidgetsByPage.TryGetValue(pageName, out var widgets))
            return NotFound(new { detail = "Page not found" });

        var widgetsNames = widgets.Select(w => w.Item2).ToList();
        return Ok(new WidgetsResponse(
            widgetsNames,
            user.CompanyTimeZone,
            pageName,
            user.ProductId));
    }

    [HttpGet("{pageName}/filters")]
    public IActionResult GetFilters(string pageName)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        if (!Pages.WidgetsByPage.TryGetValue(pageName, out _))
            return NotFound(new { detail = "Page not found" });

        var filters = BuildFilters(pageName, user.CompanyId);
        return Ok(new FiltersResponse(filters, pageName));
    }

    private List<FilterItem> BuildFilters(string pageName, int companyId)
    {
        var filters = new List<FilterItem>();

        var dataSources = _referenceData.GetDataSources(companyId);
        filters.Add(new FilterItem(
            "interactionDataSources",
            "Data Sources",
            dataSources.Select(ds => new FilterValue(ds.Name, ds.Id)).ToList()));

        filters.Add(new FilterItem(
            "interactionType",
            "Interaction Type",
            new List<FilterValue>
            {
                new("Posts", "posts"),
                new("Conversations", "conversations"),
                new("Emails", "emails"),
            }));

        var promises = _referenceData.GetSlas(companyId);
        filters.Add(new FilterItem(
            "promisesIds",
            "Promises",
            promises.Select(s => new FilterValue(s.Name, s.Id)).ToList()));

        if (pageName == "agents")
        {
            var agents = _referenceData.GetAgents(companyId);
            filters.Add(new FilterItem(
                "assigneesIds",
                "Assignees",
                agents.Select(a => new FilterValue(a.Name, a.Id)).ToList()));
        }

        return filters;
    }
}
