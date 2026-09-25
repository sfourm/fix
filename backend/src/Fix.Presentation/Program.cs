using Fix.Application;
using Fix.Infrastructure;
using Fix.Infrastructure.Persistence;
using Fix.Presentation;

var builder = WebApplication.CreateBuilder(args);

builder.Services
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
