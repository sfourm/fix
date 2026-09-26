using Fix.Storage.Application;
using Fix.Storage.Infrastructure;
using Fix.Storage.Presentation;
using Fix.Storage.Presentation.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddStorageTelemetry(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

var app = builder.Build();

app.MapPresentation();

app.Run();
