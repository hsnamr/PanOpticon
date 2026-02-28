using PanOpticon.Constants;
using PanOpticon.Models;

namespace PanOpticon.Services;

public class JobOrchestrationService
{
    private readonly RedisJobStore _redis;
    private readonly KafkaPublisher _kafka;
    private readonly ReferenceDataService _referenceData;

    public JobOrchestrationService(RedisJobStore redis, KafkaPublisher kafka, ReferenceDataService referenceData)
    {
        _redis = redis;
        _kafka = kafka;
        _referenceData = referenceData;
    }

    private static Dictionary<string, object> MapFiltersToEngine(IReadOnlyDictionary<string, object>? filters)
    {
        var result = new Dictionary<string, object>();
        if (filters == null) return result;

        foreach (var (clientKey, engineKey) in Pages.FilterKeyMapping)
        {
            if (filters.TryGetValue(clientKey, out var value) && value != null)
            {
                result[engineKey] = value;
            }
        }
        return result;
    }

    private static IReadOnlyList<string> GetInternalWidgetNames(string pageName, IReadOnlyList<string> frontEndNames)
    {
        if (!Pages.WidgetsByPage.TryGetValue(pageName, out var widgets))
            throw new ArgumentException($"Invalid page: {pageName}");

        var nameMap = widgets.ToDictionary(w => w.Item2, w => w.Item1);
        var internalNames = new List<string>();

        foreach (var fe in frontEndNames)
        {
            if (nameMap.TryGetValue(fe, out var internalName))
            {
                internalNames.Add(internalName);
            }
            else if (widgets.Any(w => w.Item1 == fe))
            {
                internalNames.Add(fe);
            }
            else
            {
                throw new ArgumentException($"Invalid widget name: {fe}");
            }
        }
        return internalNames;
    }

    private static IReadOnlyDictionary<string, string> BuildWidgetNamesMapping(string pageName, IReadOnlyList<string> internalNames)
    {
        if (!Pages.WidgetsByPage.TryGetValue(pageName, out var widgets))
            return new Dictionary<string, string>();

        var mapping = widgets.ToDictionary(w => w.Item1, w => w.Item2);
        return internalNames.ToDictionary(k => k, k => mapping.GetValueOrDefault(k, k));
    }

    public async Task<CreateJobResponse> CreateStatisticsJobAsync(
        string pageName,
        CreateJobRequest request,
        UserContext user)
    {
        if (request.StartDate > request.EndDate)
            throw new ArgumentException("Invalid date range: startDate must be <= endDate");
        if (request.EndDate - request.StartDate > 30 * 86400)
            throw new ArgumentException("Date range cannot exceed 30 days");
        if (request.WidgetsNames == null || request.WidgetsNames.Count == 0)
            throw new ArgumentException("widgetsNames cannot be empty");

        var internalNames = GetInternalWidgetNames(pageName, request.WidgetsNames);
        var widgetNamesMapping = BuildWidgetNamesMapping(pageName, internalNames);
        var engineFilters = MapFiltersToEngine(request.Filters);

        var agents = _referenceData.GetAgents(user.CompanyId);
        var promises = _referenceData.GetSlas(user.CompanyId);
        var dataSources = _referenceData.GetDataSourceIdentifiers();

        var jobId = Guid.NewGuid().ToString();
        var tabName = pageName.ToLowerInvariant();
        var productId = request.ProductId ?? user.ProductId;

        var params_ = new Dictionary<string, object>
        {
            ["jobId"] = jobId,
            ["pageName"] = pageName,
            ["widgetsNames"] = internalNames,
            ["startDate"] = request.StartDate,
            ["endDate"] = request.EndDate,
            ["filters"] = engineFilters,
            ["companyId"] = user.CompanyId,
            ["companyTimeZone"] = user.CompanyTimeZone,
            ["productId"] = productId,
            ["featureId"] = user.FeatureId,
            ["pageId"] = user.PageId,
            ["tabName"] = tabName,
        };

        await _redis.CreateJobAsync(
            user.CompanyId,
            jobId,
            pageName,
            internalNames,
            params_);

        var payload = new Dictionary<string, object>
        {
            ["job_id"] = jobId,
            ["page_name"] = pageName,
            ["widgets_names"] = internalNames,
            ["widget_names_mapping"] = widgetNamesMapping,
            ["trackers_list"] = new Dictionary<string, object>(),
            ["promises_ids"] = promises.Select(s => s.Id).ToList(),
            ["users_ids"] = agents.Select(a => a.Id).ToList(),
            ["start_date"] = request.StartDate,
            ["end_date"] = request.EndDate,
            ["filters"] = System.Text.Json.JsonSerializer.Serialize(engineFilters),
            ["data_sources"] = dataSources,
            ["company_id"] = user.CompanyId,
            ["company_time_zone"] = user.CompanyTimeZone,
            ["feature_id"] = user.FeatureId,
            ["page_id"] = user.PageId,
            ["product"] = "PUBLIC_API",
            ["tab_name"] = tabName,
        };

        _kafka.PublishStatisticsJob(payload);

        return new CreateJobResponse(new
        {
            jobId,
            pageName,
            widgetsNames = request.WidgetsNames,
        });
    }

    public async Task<PollJobResponse> PollJobAsync(string jobId, string pageName, int companyId)
    {
        var params_ = await _redis.GetJobParamsAsync(companyId, jobId);
        if (params_ == null)
            throw new KeyNotFoundException("JOB_ID_NOT_FOUND");

        var storedPageName = params_.PageName;
        if (storedPageName != pageName)
            throw new ArgumentException("PAGE_NAME_MISMATCH");

        var expectedCount = await _redis.GetExpectedWidgetCountAsync(companyId, jobId);
        var metrics = await _redis.GetWidgetMetricsAsync(companyId, jobId);

        if (metrics.Count >= expectedCount && expectedCount > 0)
        {
            await _redis.ExpireJobAsync(companyId, jobId);
            var widgetData = metrics.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
            return new PollJobResponse(new
            {
                dataAvailable = true,
                widgetData,
            });
        }

        return new PollJobResponse(new
        {
            dataAvailable = false,
            widgetData = new Dictionary<string, object>(),
        });
    }
}
