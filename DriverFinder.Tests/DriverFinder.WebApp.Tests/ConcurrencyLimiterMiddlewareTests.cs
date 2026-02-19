using System.Net;
using DriverFinder.WebApp.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace DriverFinder.WebApp.Tests;

public class DelayMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        await Task.Delay(2000);
    }
}

[TestFixture]
public class ConcurrencyLimiterMiddlewareTests
{
    private WebApplicationFactory<Program> factory;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSolutionRelativeContentRoot("DriverFinder.WebApp");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Settings:ParallelLimit"] = "10"
                    });
                });

                builder.Configure((context, app) =>
                {
                    app.UseMiddleware<ConcurrencyLimiterMiddleware>();

                    app.UseMiddleware<DelayMiddleware>();

                    app.UseSwagger();
                    app.UseSwaggerUI();
                });
            });
    }

    [Test]
    public async Task WhenSendingMassiveConcurrentRequests_ShouldThrottleExcessRequests()
    {
        const int parallelLimit = 10;
        const int extraRequests = 5;
        var client = factory.CreateClient();

        //отправляем первые 10 запросов
        var initialTasks = Enumerable.Range(0, parallelLimit)
            .Select(_ => client.GetAsync("/swagger/index.html"))
            .ToList();

        await Task.Delay(500);

        //отправляем излишние запросы
        var extraTasks = Enumerable.Range(0, extraRequests)
            .Select(_ => client.GetAsync("/swagger/index.html"))
            .ToList();

        var extraResponses = await Task.WhenAll(extraTasks);
        var initialResponses = await Task.WhenAll(initialTasks);

        
        Assert.Multiple(() =>
        {
            Assert.That(initialResponses.All(r => r.StatusCode == HttpStatusCode.OK),
                Is.True,
                "Первые 10 должны быть OK.");

            Assert.That(extraResponses.All(r => r.StatusCode == HttpStatusCode.ServiceUnavailable),
                Is.True,
                "Лишние должны быть 503.");
        });
    }
}