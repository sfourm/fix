using Fix.Application;
using Fix.Infrastructure;
using Fix.Infrastructure.Persistence;
using Fix.Presentation;
using Fix.Presentation.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddBackendTelemetry(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

var app = builder.Build();

if (app.Configuration.GetValue("Database:MigrateOnStartup", app.Environment.IsDevelopment()))
{
    await app.Services.InitializeDatabaseAsync();
}

app.MapPresentation();

app.Run();
