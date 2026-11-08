using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
    private readonly IMongoCollection<SpeedAlert> _alertCollection; 

    public VehicleConsumerWorker(
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<RedisOptions> redisOptions,
        IOptions<MongoDbOptions> mongoOptions, 
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

        var mongoClient = new MongoClient(mongoOptions.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoOptions.Value.DatabaseName);
        _alertCollection = mongoDatabase.GetCollection<SpeedAlert>(mongoOptions.Value.CollectionName);
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

                    if (location.Speed > 100)
                    {
                        var alert = new SpeedAlert
                        {
                            VehicleId = location.VehicleId,
                            Speed = location.Speed,
                            Message = $" سرعت غیرمجاز! ماشین {location.VehicleId} با سرعت {location.Speed} در حال حرکت است."
                        };

                        await _alertCollection.InsertOneAsync(alert, cancellationToken: stoppingToken);
                        _logger.LogWarning(alert.Message);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer stopped.");
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _redis.Dispose();
        }
    }
}