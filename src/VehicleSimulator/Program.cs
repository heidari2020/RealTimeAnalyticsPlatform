using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VehicleSimulator.Configuration;
using VehicleSimulator.Services;
using VehicleSimulator.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables();

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection(KafkaOptions.SectionName));

builder.Services.Configure<SimulationOptions>(
    builder.Configuration.GetSection(SimulationOptions.SectionName));

builder.Services.AddSingleton<IVehicleLocationProducer, VehicleLocationProducer>();
builder.Services.AddHostedService<VehicleSimulationWorker>();

var host = builder.Build();
await host.RunAsync();