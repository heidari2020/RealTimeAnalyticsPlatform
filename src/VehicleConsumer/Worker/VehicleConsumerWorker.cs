using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleConsumer.Configuration;
namespace VehicleConsumer.Worker;
public sealed class VehicleConsumerWorker : BackgroundService
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly ILogger<VehicleConsumerWorker> _logger;
    private readonly string _topic;

    public VehicleConsumerWorker(
        IOptions<KafkaOptions> options,
        ILogger<VehicleConsumerWorker> logger)
    {
        _logger = logger;
        _topic = options.Value.Topic;

        var config = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest 
        };

        _consumer = new ConsumerBuilder<Null, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Vehicle Consumer started. Listening to {Topic}...", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult?.Message?.Value is null) continue;

                _logger.LogInformation("Received: {Message}", consumeResult.Message.Value);
                //todo
                await Task.Delay(10, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer stopped gracefully.");
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
        }
    }
}