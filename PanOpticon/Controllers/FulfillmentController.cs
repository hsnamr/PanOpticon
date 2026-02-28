using Microsoft.AspNetCore.Mvc;
using PanOpticon.Models;
using PanOpticon.Services;

namespace PanOpticon.Controllers;

[ApiController]
[Route("statistics")]
public class FulfillmentController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly KafkaPublisher _kafka;

    private static readonly SurveyFulfillmentQuestion[] MockFulfillmentQuestions =
    {
        new(1, "How satisfied are you?", "fediverse_private"),
        new(2, "Would you recommend us?", "signal_private"),
    };

    public FulfillmentController(AuthService auth, KafkaPublisher kafka)
    {
        _auth = auth;
        _kafka = kafka;
    }

    private UserContext? GetUser()
    {
        var authToken = Request.Headers["auth-token"].FirstOrDefault();
        var authorization = Request.Headers["Authorization"].FirstOrDefault();
        return _auth.GetCurrentUser(authToken, authorization);
    }

    [HttpPost("{pageName}/fulfillment_response_counts")]
    public IActionResult FulfillmentResponseCounts(string pageName, [FromBody] FulfillmentResponseCountsRequest request)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        if (pageName != "user-survey")
            return NotFound(new { detail = "Page not found" });

        var questionIds = string.IsNullOrEmpty(request.QuestionIds)
            ? (IReadOnlyList<string>)Array.Empty<string>()
            : request.QuestionIds.Split(',').Select(q => q.Trim()).Where(q => !string.IsNullOrEmpty(q)).ToList();

        if (questionIds.Count > 20)
            return BadRequest(new { detail = "Maximum 20 questionIds allowed" });

        var jobId = Guid.NewGuid().ToString();
        var payload = new
        {
            job_id = jobId,
            page_name = pageName,
            question_ids = questionIds,
            data_source_name = request.DataSourceName,
            start_date = request.StartDate,
            end_date = request.EndDate,
            filters = request.Filters ?? new Dictionary<string, object>(),
            company_id = user.CompanyId,
        };
        _kafka.PublishFulfillmentJob(payload);

        return Ok(new
        {
            data = new
            {
                jobId,
                pageName,
                questionIds,
                dataSourceName = request.DataSourceName,
                fulfillmentQuestions = Array.Empty<object>(),
            },
        });
    }

    [HttpPost("{pageName}/fulfillment_question_responses")]
    public IActionResult FulfillmentQuestionResponses(string pageName, [FromBody] FulfillmentQuestionResponsesRequest request)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        if (pageName != "user-survey")
            return NotFound(new { detail = "Page not found" });

        var jobId = Guid.NewGuid().ToString();
        var payload = new
        {
            job_id = jobId,
            page_name = pageName,
            question_id = request.QuestionId,
            page_number = request.PageNumber ?? 1,
            data_source_name = request.DataSourceName,
            start_date = request.StartDate,
            end_date = request.EndDate,
            filters = request.Filters ?? new Dictionary<string, object>(),
            company_id = user.CompanyId,
        };
        _kafka.PublishFulfillmentJob(payload);

        return Ok(new
        {
            data = new
            {
                jobId,
                pageName,
                questionId = request.QuestionId,
                pageNumber = request.PageNumber ?? 1,
                dataSourceName = request.DataSourceName,
            },
        });
    }

    [HttpPost("{pageName}/fulfillment_user_responses")]
    public IActionResult FulfillmentUserResponses(string pageName, [FromBody] FulfillmentUserResponsesRequest request)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        if (pageName != "user-survey")
            return NotFound(new { detail = "Page not found" });

        return Ok(new
        {
            data = new
            {
                jobId = Guid.NewGuid().ToString(),
                pageName,
                fulfillmentUserSurveys = request.FulfillmentUserSurveys ?? Array.Empty<object>(),
            },
        });
    }

    [HttpGet("{pageName}/fulfillment_questions")]
    public IActionResult FulfillmentQuestions(string pageName, [FromQuery] string dataSourceName, [FromQuery] string? search, [FromQuery] int page = 1)
    {
        var user = GetUser();
        if (user == null) return Unauthorized(new { detail = "Invalid or missing token" });

        if (pageName != "user-survey")
            return NotFound(new { detail = "Page not found" });

        var questions = MockFulfillmentQuestions
            .Where(q => q.DataSourceName == dataSourceName)
            .ToList();

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLowerInvariant();
            questions = questions.Where(q => q.Question.ToLowerInvariant().Contains(searchLower)).ToList();
        }

        const int perPage = 10;
        var total = questions.Count;
        var items = questions
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();

        return Ok(new
        {
            data = items,
            pagination = new { page, perPage, total },
            dataSourceName,
        });
    }
}
