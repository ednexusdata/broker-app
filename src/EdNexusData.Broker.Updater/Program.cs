using EdNexusData.Broker.Updater;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddSystemd();

var host = builder.Build();
host.Run();
