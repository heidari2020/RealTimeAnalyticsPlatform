namespace VehicleSimulator.Configuration;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}

public sealed class SimulationOptions
{
    public const string SectionName = "Simulation";

    public int IntervalInSeconds { get; set; } = 5;
    public string[] VehicleIds { get; set; } = [];
}