using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleSimulator.Configuration;
using VehicleSimulator.Models;
using VehicleSimulator.Services;

namespace VehicleSimulator.Workers;

public sealed class VehicleSimulationWorker : BackgroundService
{
    private readonly IVehicleLocationProducer _producer;
    private readonly SimulationOptions _simulationOptions;
    private readonly ILogger<VehicleSimulationWorker> _logger;
    private readonly Random _random = new();

    public VehicleSimulationWorker(
        IVehicleLocationProducer producer,
        IOptions<SimulationOptions> simulationOptions,
        ILogger<VehicleSimulationWorker> logger)
    {
        _producer = producer;
        _simulationOptions = simulationOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vehicle simulation started. Publishing every {Interval}s",
            _simulationOptions.IntervalInSeconds);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_simulationOptions.IntervalInSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishBatchAsync(stoppingToken);
        }

        _logger.LogInformation("Vehicle simulation stopped.");
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        var tasks = _simulationOptions.VehicleIds
            .Select(vehicleId => _producer.PublishAsync(GenerateLocation(vehicleId), cancellationToken));

        await Task.WhenAll(tasks);
    }

    private VehicleLocation GenerateLocation(string vehicleId) => new()
    {
        VehicleId = vehicleId,
        // Tehran city center coordinates (~5 km radius around the city)
        // Base: 35.7°N, 51.4°E — Random offset up to 0.05° (~5 km)
        Latitude = 35.7 + (_random.NextDouble() * 0.05),
        Longitude = 51.4 + (_random.NextDouble() * 0.05),
        Speed = _random.Next(0, 120),
        Timestamp = DateTime.UtcNow,
        Status = _random.Next(0, 10) switch
        {
            < 2 => "idle",
            > 8 => "stopped",
            _ => "moving"
        }
    };
}