using GIT_Backend.Application.Service;
using GIT_Backend.Application.Worker;
using GIT_Backend.Controllers;
using GIT_Backend.Infra;
using GIT_Backend.Infra.Database;
using GIT_Backend.Infra.Middleware;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Bootstrap log
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // Real Log
    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services));

    // Environments load
    var envLoader = new EnvironmentLoader();
    var connectionString = envLoader.LoadMainDBConnectionString();
    var redisConnectionString = envLoader.LoadRedisConnectionString();
    var internalApiKey = envLoader.LoadInternalApiKey();

    builder.Services.AddDbContext<GITDBContext>(options =>
        options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

    // Add Services For DI
    builder.Services.AddScoped<CrawlerService>();
    builder.Services.AddScoped<AnalyzerService>();

    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(redisConnectionString));
    builder.Services.AddHostedService<CrawlContentsConsumer>();
    builder.Services.AddHostedService<AnalyzeJobDispatcher>();
    builder.Services.AddHostedService<AnalyzedContentsConsumer>();

    builder.Services.AddControllers();

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddOpenApi();

    var app = builder.Build();
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();
    app.UseWhen(
        context =>
            context.Request.Path.StartsWithSegments("/api/crawler", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/api/analyzer", StringComparison.OrdinalIgnoreCase),
        internalApiApp =>
        {
            internalApiApp.UseMiddleware<InternalApiAuthorizationMiddleware>(internalApiKey);
        });

    app.UseExceptionHandler();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is HostAbortedException || ex.GetType().Name == "StopTheHostException")
{
    // EF Core design-time tooling host abort
}
finally
{
    Log.CloseAndFlush();
}
