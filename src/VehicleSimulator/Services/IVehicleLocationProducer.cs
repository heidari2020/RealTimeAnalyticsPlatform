using VehicleSimulator.Models;

namespace VehicleSimulator.Services;

public interface IVehicleLocationProducer
{
    Task PublishAsync(VehicleLocation location, CancellationToken cancellationToken = default);
}