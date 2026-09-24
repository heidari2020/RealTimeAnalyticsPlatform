namespace VehicleSimulator.Models;

public sealed record VehicleLocation
{
    public required string VehicleId { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required double Speed { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string Status { get; init; }
}