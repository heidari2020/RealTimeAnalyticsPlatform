using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleConsumer.Models;
public class SpeedAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string VehicleId { get; set; } = string.Empty;
    public double Speed { get; set; }
    public DateTime AlertTime { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = string.Empty;
}
