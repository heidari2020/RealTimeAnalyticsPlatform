using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var redis = ConnectionMultiplexer.Connect("localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/vehicles/{vehicleId}", async (string vehicleId, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var data = await db.StringGetAsync($"vehicle:{vehicleId}");

    if (data.IsNullOrEmpty)
        return Results.NotFound($"Vehicle {vehicleId} not found in cache.");

    return Results.Ok(JsonSerializer.Deserialize<object>(data!));
})
.WithName("GetVehicleLocation")
.WithSummary("Get the latest location of a vehicle from Redis");

app.Run();