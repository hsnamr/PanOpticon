namespace PanOpticon;

public class PanOpticonOptions
{
    public const string SectionName = "PanOpticon";

    public string RedisUrl { get; set; } = "redis://localhost:6379/0";
    public int RedisJobTtl { get; set; } = 86400;
    public int RedisCompletionTtl { get; set; } = 3600;

    public string KafkaBootstrapServers { get; set; } = "localhost:9092";
    public string KafkaStatisticsTopic { get; set; } = "public_api_panopticon_statistics_publisher_engine";
    public string KafkaFulfillmentTopic { get; set; } = "ods_export_fulfillment_questions_responses_count_v0";

    public string MinioEndpoint { get; set; } = "localhost:9000";
    public string MinioAccessKey { get; set; } = "minioadmin";
    public string MinioSecretKey { get; set; } = "minioadmin";
    public string MinioBucket { get; set; } = "panopticon-reports";
    public bool MinioSecure { get; set; } = false;

    public string AuthTokenHeader { get; set; } = "auth-token";
    public int DefaultCompanyId { get; set; } = 1;
    public string DefaultCompanyTimeZone { get; set; } = "UTC";
    public int DefaultProductId { get; set; } = 1;
    public int DefaultFeatureId { get; set; } = 10;
    public int DefaultPageId { get; set; } = 1;
}
