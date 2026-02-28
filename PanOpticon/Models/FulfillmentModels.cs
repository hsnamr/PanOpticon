namespace PanOpticon.Models;

public record FulfillmentResponseCountsRequest(
    long StartDate,
    long EndDate,
    string DataSourceName,
    IReadOnlyDictionary<string, object>? Filters = null,
    string? QuestionIds = null);

public record FulfillmentQuestionResponsesRequest(
    long StartDate,
    long EndDate,
    string DataSourceName,
    string QuestionId,
    int? PageNumber = null,
    IReadOnlyDictionary<string, object>? Filters = null);

public record FulfillmentUserResponsesRequest(
    long StartDate,
    long EndDate,
    IReadOnlyList<object>? FulfillmentUserSurveys = null,
    IReadOnlyDictionary<string, object>? Filters = null);

public record SurveyFulfillmentQuestion(int Id, string Question, string? DataSourceName = null);
