using PanOpticon.Constants;
using PanOpticon.Models;

namespace PanOpticon.Services;

/// <summary>
/// Reference data - agents, teams, data sources (Fediverse, Signal), Promises, routings.
/// </summary>
public class ReferenceDataService
{
    private static readonly AgentItem[] MockAgents =
    {
        new(1, "Agent One", "active"),
        new(2, "Agent Two", "active"),
    };

    private static readonly TeamItem[] MockTeams =
    {
        new(1, "Support Team"),
        new(2, "Sales Team"),
    };

    private static readonly DataSourceItem[] MockDataSources =
    {
        new(1, "fediverse_public"),
        new(2, "fediverse_private"),
        new(3, "signal_private"),
    };

    private static readonly SlaItem[] MockSlas =
    {
        new(1, "Standard Promise",
            new SlaTimeValue(60, "minutes"),
            new SlaTimeValue(30, "minutes"),
            new SlaTimeValue(24, "hours"),
            new SlaTimeValue(15, "minutes")),
    };

    private static readonly RoutingItem[] MockRoutings =
    {
        new(1, "Default Routing"),
        new(2, "Priority Routing"),
    };

    public IReadOnlyList<AgentItem> GetAgents(int companyId) => MockAgents;
    public IReadOnlyList<TeamItem> GetTeams(int companyId) => MockTeams;
    public IReadOnlyList<DataSourceItem> GetDataSources(int companyId) => MockDataSources;
    public IReadOnlyList<SlaItem> GetSlas(int companyId) => MockSlas;
    public IReadOnlyList<RoutingItem> GetRoutings(int companyId) => MockRoutings;
    public IReadOnlyList<string> GetDataSourceIdentifiers() => Pages.DataSourceIdentifiers.ToList();
}
