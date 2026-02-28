namespace PanOpticon.Models;

public record CreateJobRequest(
    IReadOnlyList<string> WidgetsNames,
    long StartDate,
    long EndDate,
    IReadOnlyDictionary<string, object>? Filters = null,
    int? ProductId = null);

public record CreateJobResponse(object Data);

public record PollJobResponse(object Data);
