using System.Text.Json;
using Microsoft.Extensions.Options;
using PanOpticon.Models;
using StackExchange.Redis;

namespace PanOpticon.Services;


public class RedisJobStore
{
    private readonly PanOpticonOptions _options;
    private ConnectionMultiplexer? _connection;
    private IDatabase? _db;

    public RedisJobStore(IOptions<PanOpticonOptions> options)
    {
        _options = options.Value;
    }

    public async Task ConnectAsync()
    {
        _connection = await ConnectionMultiplexer.ConnectAsync(_options.RedisUrl);
        _db = _connection.GetDatabase();
    }

    public void Close()
    {
        _connection?.Dispose();
        _connection = null;
        _db = null;
    }

    private static string MainKey(int companyId, string jobId) =>
        $"PublicApis:{companyId}:ENGAGEMENTS:{jobId}";

    public async Task CreateJobAsync(int companyId, string jobId, string pageName, IReadOnlyList<string> widgetsNames, Dictionary<string, object> params_)
    {
        if (_db == null) throw new InvalidOperationException("Redis not connected");

        var mainKey = MainKey(companyId, jobId);
        var creationDay = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var creationDayKey = $"{mainKey}:CREATION_DAY";

        var paramsJson = JsonSerializer.Serialize(new Dictionary<string, object>(params_)
        {
            ["pageName"] = pageName,
            ["widgetsNames"] = widgetsNames,
        });

        await _db.HashSetAsync(mainKey, creationDay, widgetsNames.Count.ToString());
        await _db.HashSetAsync(mainKey, "job_id_params", paramsJson);
        await _db.StringSetAsync(creationDayKey, creationDay);
        await _db.KeyExpireAsync(mainKey, TimeSpan.FromSeconds(_options.RedisJobTtl));
        await _db.KeyExpireAsync(creationDayKey, TimeSpan.FromSeconds(_options.RedisJobTtl));
    }

    public async Task<JobParams?> GetJobParamsAsync(int companyId, string jobId)
    {
        if (_db == null) return null;

        var mainKey = MainKey(companyId, jobId);
        var raw = await _db.HashGetAsync(mainKey, "job_id_params");
        if (raw.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<JobParams>(raw.ToString()!);
    }

    public async Task<string?> GetCreationDayAsync(int companyId, string jobId)
    {
        if (_db == null) return null;

        var creationDayKey = $"{MainKey(companyId, jobId)}:CREATION_DAY";
        var raw = await _db.StringGetAsync(creationDayKey);
        return raw.IsNullOrEmpty ? null : raw.ToString();
    }

    public async Task<int> GetExpectedWidgetCountAsync(int companyId, string jobId)
    {
        var creationDay = await GetCreationDayAsync(companyId, jobId);
        if (string.IsNullOrEmpty(creationDay) || _db == null) return 0;

        var mainKey = MainKey(companyId, jobId);
        var count = await _db.HashGetAsync(mainKey, creationDay);
        return int.TryParse(count.ToString(), out var n) ? n : 0;
    }

    public async Task<IReadOnlyDictionary<string, JsonElement>> GetWidgetMetricsAsync(int companyId, string jobId)
    {
        if (_db == null) return new Dictionary<string, JsonElement>();

        var metricsKey = $"{MainKey(companyId, jobId)}:metrics";
        var raw = await _db.HashGetAllAsync(metricsKey);
        var result = new Dictionary<string, JsonElement>();
        foreach (var entry in raw)
        {
            if (!entry.Value.IsNullOrEmpty)
            {
                var value = JsonSerializer.Deserialize<JsonElement>(entry.Value!.ToString()!);
                result[entry.Name!] = value;
            }
        }
        return result;
    }

    public async Task ExpireJobAsync(int companyId, string jobId)
    {
        if (_db == null) return;

        var mainKey = MainKey(companyId, jobId);
        var metricsKey = $"{mainKey}:metrics";
        var creationDayKey = $"{mainKey}:CREATION_DAY";
        var ttl = TimeSpan.FromSeconds(_options.RedisCompletionTtl);

        await Task.WhenAll(
            _db.KeyExpireAsync(mainKey, ttl),
            _db.KeyExpireAsync(metricsKey, ttl),
            _db.KeyExpireAsync(creationDayKey, ttl));
    }
}
