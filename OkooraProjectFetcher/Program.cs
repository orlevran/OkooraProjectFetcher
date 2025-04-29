using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OkooraProjectFetcher.BackgroundServices;
using OkooraProjectFetcher.Controllers;
using OkooraProjectFetcher.Services;
internal class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services
        builder.Services.AddControllers();
        builder.Services.AddSingleton<ExchangeServiceReader>();
        builder.Services.AddSingleton<ExchangeRateMongoRepository>();
        builder.Services.AddSingleton<ExchangeRateBackgroundService>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<ExchangeRateBackgroundService>());

        var app = builder.Build();

        // Map routes
        app.MapControllers();

        // Run web server
        app.Run();
    }
}