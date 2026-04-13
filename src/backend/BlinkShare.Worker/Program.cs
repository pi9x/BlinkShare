using BlinkShare.Worker;
using BlinkShare.Worker.Features.CleanupObjects;
using BlinkShare.Worker.Features.ExpireShares;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBlinkShareWorkerInfrastructure(builder.Configuration);
builder.Services.AddExpireSharesFeature(builder.Configuration);
builder.Services.AddCleanupObjectsFeature(builder.Configuration);

var host = builder.Build();

host.Run();
