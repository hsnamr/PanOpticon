using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace PanOpticon.Services;

public class KafkaPublisher
{
    private readonly PanOpticonOptions _options;
    private IProducer<Null, string>? _producer;

    public KafkaPublisher(IOptions<PanOpticonOptions> options)
    {
        _options = options.Value;
    }

    private IProducer<Null, string> GetProducer()
    {
        if (_producer != null) return _producer;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.KafkaBootstrapServers,
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
        return _producer;
    }

    public void PublishStatisticsJob(object payload)
    {
        var producer = GetProducer();
        var json = JsonSerializer.Serialize(payload);
        producer.Produce(_options.KafkaStatisticsTopic, new Message<Null, string> { Value = json });
    }

    public void PublishFulfillmentJob(object payload)
    {
        var producer = GetProducer();
        var json = JsonSerializer.Serialize(payload);
        producer.Produce(_options.KafkaFulfillmentTopic, new Message<Null, string> { Value = json });
    }

    public void Close()
    {
        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
        _producer = null;
    }
}
