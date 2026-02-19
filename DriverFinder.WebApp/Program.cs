using DriverFinder.Lib.Finders;
using DriverFinder.WebApp.Middleware;
using DriverFinder.WebApp.Services;
using DriverFinder.WebApp.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MapSettings>(builder.Configuration.GetSection("MapSettings"));
builder.Services.Configure<ParallelLimitSettings>(builder.Configuration.GetSection("Settings"));

builder.Services.AddSingleton<DriverService>();
builder.Services.AddSingleton<StatefulGridDriverFinder>(sp =>
{
    var mapSettings = sp.GetRequiredService<IOptions<MapSettings>>().Value;
    return new StatefulGridDriverFinder(mapSettings.GridCellSize);
});
builder.Services.AddScoped<OrderService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ConcurrencyLimiterMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
