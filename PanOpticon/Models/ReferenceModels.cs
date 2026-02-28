namespace PanOpticon.Models;

public record AgentItem(int Id, string Name, string Status);

public record TeamItem(int Id, string Name);

public record DataSourceItem(int Id, string Name);

public record SlaTimeValue(object Value, string Unit);

public record SlaItem(
    int Id,
    string Name,
    SlaTimeValue? FirstResponseTime = null,
    SlaTimeValue? NextResponseTime = null,
    SlaTimeValue? TimeToComplete = null,
    SlaTimeValue? TotalUnassignedTime = null);

public record RoutingItem(int Id, string Name);
