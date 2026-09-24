using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleSimulator.Configuration;
using VehicleSimulator.Models;

namespace VehicleSimulator.Services;

public sealed class VehicleLocationProducer : IVehicleLocationProducer, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<VehicleLocationProducer> _logger;
    private readonly string _topic;

    public VehicleLocationProducer(
        IOptions<KafkaOptions> options,
        ILogger<VehicleLocationProducer> logger)
    {
        _logger = logger;
        _topic = options.Value.Topic;

        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            ClientId = options.Value.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<Null, string>(config)
            .SetErrorHandler((_, error) =>
                _logger.LogError("Kafka error: {Reason}", error.Reason))
            .Build();
    }

    public async Task PublishAsync(VehicleLocation location, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(location);

        try
        {
            var result = await _producer.ProduceAsync(
                _topic,
                new Message<Null, string> { Value = payload },
                cancellationToken);

            _logger.LogInformation(
                "Published vehicle {VehicleId} to {Topic} [Partition: {Partition}, Offset: {Offset}]",
                location.VehicleId,
                _topic,
                result.Partition.Value,
                result.Offset.Value);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Failed to publish vehicle {VehicleId}", location.VehicleId);
        }
    }

    public void Dispose() => _producer.Dispose();
}