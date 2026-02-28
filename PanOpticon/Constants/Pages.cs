namespace PanOpticon.Constants;

/// <summary>
/// Page and widget definitions. Fediverse (ActivityPub) and Signal.
/// </summary>
public static class Pages
{
    public static readonly IReadOnlyList<PageInfo> All = new[]
    {
        new PageInfo(1, "inbox"),
        new PageInfo(2, "promises"),
        new PageInfo(3, "agents"),
        new PageInfo(4, "user-survey"),
    };

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<(string InternalName, string FrontEndName)>> WidgetsByPage = new Dictionary<string, IReadOnlyList<(string, string)>>
    {
        ["inbox"] = new[]
        {
            ("interaction_statistics_v1_overview", "Inbox Overview"),
            ("interaction_statistics_v1_completed_overtime", "Interactions Completed Overtime"),
            ("interaction_statistics_v1_volume", "Interactions Volume"),
            ("interaction_statistics_v1_engagers_activity", "Engagers Activity (Unique)"),
            ("interaction_statistics_v1_agents_performance", "Agents Performance"),
            ("interaction_statistics_v1_average_promises", "Average Promises"),
            ("interaction_statistics_v1_data_sources", "Data Sources"),
            ("interaction_statistics_v1_teams", "Interactions Completed By Teams"),
            ("interaction_statistics_v1_completion_reason", "Completion Reason"),
            ("interaction_statistics_v1_routings", "Interaction Distribution Over Routings"),
            ("interaction_statistics_v1_tags_overtime", "Tags Usage Over Time"),
            ("interaction_statistics_v1_tags_distribution", "Tags Distribution Over Interactions"),
        },
        ["promises"] = new[]
        {
            ("promise_statistics_v1_overview", "Average Promises Overview"),
            ("promise_statistics_v1_overtime", "Interactions Overtime"),
            ("promise_statistics_v1_performance", "Promises Performance"),
            ("promise_statistics_v1_time_distribution", "Promises Time Distribution"),
            ("promise_statistics_v1_misses_activity", "Misses Activity"),
            ("promise_statistics_v1_hits_activity", "Hits Activity"),
        },
        ["agents"] = new[]
        {
            ("agent_statistics_v1_performance", "Agents Performance"),
            ("agent_statistics_v1_interactions", "Interactions Statistics"),
            ("agent_statistics_v1_distribution", "Agents Distribution"),
            ("agent_statistics_v1_inbox_performance", "Agent Inbox Performance"),
            ("agent_statistics_v1_overview", "Overview"),
            ("agent_statistics_v1_promises", "Promises Hits & Misses"),
            ("agent_statistics_v1_csat_score", "Agents Fulfillment Score"),
            ("agent_statistics_v1_csat_overtime", "Agent Fulfillment Score Overtime"),
            ("agent_statistics_v1_status_overview", "Agent Status Overview"),
            ("agent_statistics_v1_status_summary", "Agent Status Summary"),
        },
        ["user-survey"] = new[]
        {
            ("csat_questions_responses_count", "Fulfillment Questions Responses Count"),
            ("csat_question_responses", "Fulfillment Question Responses"),
            ("csat_user_responses", "Fulfillment User Responses"),
        },
    };

    public static readonly IReadOnlyDictionary<string, string> FilterKeyMapping = new Dictionary<string, string>
    {
        ["interactionDataSources"] = "data_sources",
        ["interactionStatisticsMonitors"] = "monitors",
        ["interactionType"] = "interaction_types",
        ["interactionRouting"] = "routings_ids",
        ["interactionTags"] = "tags_ids",
        ["promisesIds"] = "promises_ids",
        ["assigneesIds"] = "assignees_ids",
    };

    public static readonly IReadOnlyList<string> DataSourceIdentifiers = new[]
    {
        "fediverse_public",
        "fediverse_private",
        "signal_private",
    };
}

public record PageInfo(int Id, string Name);
