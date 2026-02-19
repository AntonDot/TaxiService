using System.Net;
using DriverFinder.WebApp.Settings;
using Microsoft.Extensions.Options;

namespace DriverFinder.WebApp.Middleware;

public class ConcurrencyLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SemaphoreSlim _semaphore;

    public ConcurrencyLimiterMiddleware(RequestDelegate next, IOptions<ParallelLimitSettings> options)
    {
        _next = next;
        var parallelLimit = options.Value.ParallelLimit;
        _semaphore = new SemaphoreSlim(parallelLimit, parallelLimit);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!await _semaphore.WaitAsync(0))
        {
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await context.Response.WriteAsync("Service is unavailable. Too many requests. Please try again later.");
            return;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
