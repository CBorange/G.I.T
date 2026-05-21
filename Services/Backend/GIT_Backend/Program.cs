using GIT_Backend.Infra;
using GIT_Backend.Application.Service;
using GIT_Backend.Infra.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Log Settings

// Bootstrap log
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

// Real Log
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

// DB Connection
var secretsLoader = new SecretsLoader();
var connectionString = secretsLoader.LoadConnectionString();

builder.Services.AddDbContext<GITDBContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

// Add Services For DI
builder.Services.AddScoped<CrawlerService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
