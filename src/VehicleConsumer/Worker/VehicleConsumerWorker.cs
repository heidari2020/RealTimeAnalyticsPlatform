using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VehicleConsumer.Configuration;
using VehicleConsumer.Models; 

namespace VehicleConsumer.Worker;

public sealed class VehicleConsumerWorker : BackgroundService
{
    private readonly IConsumer<Null, string> _consumer;
    private readonly ILogger<VehicleConsumerWorker> _logger;
    private readonly string _topic;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _redisDb;

    public VehicleConsumerWorker(
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<RedisOptions> redisOptions,
        ILogger<VehicleConsumerWorker> logger)
    {
        _logger = logger;
        _topic = kafkaOptions.Value.Topic;

        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.Value.BootstrapServers,
            GroupId = kafkaOptions.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Null, string>(config).Build();
        _redis = ConnectionMultiplexer.Connect(redisOptions.Value.ConnectionString);
        _redisDb = _redis.GetDatabase();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Vehicle Consumer started. Listening to {Topic} and saving to Redis...", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                if (consumeResult?.Message?.Value is null) continue;

                var location = JsonSerializer.Deserialize<VehicleLocation>(consumeResult.Message.Value);
                if (location != null)
                {
                    var key = $"vehicle:{location.VehicleId}";
                    await _redisDb.StringSetAsync(key, consumeResult.Message.Value);

                    _logger.LogInformation("Saved {VehicleId} to Redis (Speed: {Speed})", location.VehicleId, location.Speed);
                }
            }
        }
        catch (OperationCanceledException) { _logger.LogInformation("Consumer stopped."); }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _redis.Dispose();
        }
    }
}