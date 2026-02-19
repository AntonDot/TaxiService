using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;
using DriverFinder.WebApp.Exceptions;
using DriverFinder.WebApp.Settings;
using Microsoft.Extensions.Options;

namespace DriverFinder.WebApp.Services;

public class OrderService(
    IOptions<MapSettings> mapSettings,
    DriverService driverService,
    StatefulGridDriverFinder driverFinder)
{
    private readonly MapSettings mapSettings = mapSettings.Value;
    private readonly HttpClient httpClient = new();

    public async Task<(Driver? driver, int distance, List<(int, int)>? route)> FindDriverForOrder(Order order)
    {
        if (order.Location.X < 0 || order.Location.X >= mapSettings.N || order.Location.Y < 0 || order.Location.Y >= mapSettings.M)
        {
            throw new InvalidCoordinatesException("Координаты некорректны");
        }

        if (driverService.GetDrivers().Count == 0)
        {
            throw new NoDriversAvailableException("Свободных водителей нет");
        }

        var candidateDrivers = driverFinder.FindNearest(order, 10);
        if (candidateDrivers.Count == 0)
        {
            throw new NoDriversAvailableException("Не удалось найти подходящих водителей поблизости");
        }
        
        var bestDistance = Math.Abs(candidateDrivers.First().Location.X - order.Location.X) + Math.Abs(candidateDrivers.First().Location.Y - order.Location.Y);
        var bestDrivers = candidateDrivers.Where(d => Math.Abs(d.Location.X - order.Location.X) + Math.Abs(d.Location.Y - order.Location.Y) == bestDistance).ToList();

        int randomIndex;
        try
        {
            var response = await httpClient.GetStringAsync("http://www.randomnumberapi.com/api/v1.0/random?min=0&max=" + (bestDrivers.Count - 1) + "&count=1");
            randomIndex = int.Parse(response.Trim('[', ']'));
        }
        catch
        {
            randomIndex = new Random().Next(0, bestDrivers.Count);
        }

        var selectedDriver = bestDrivers[randomIndex];
        var route = GetRoute(selectedDriver, order);
        return (selectedDriver, (int)bestDistance, route);
    }

    private static List<(int, int)> GetRoute(Driver driver, Order order)
    {
        var route = new List<(int, int)>();
        var currentX = driver.Location.X;
        var currentY = driver.Location.Y;

        while (currentX != order.Location.X || currentY != order.Location.Y)
        {
            if (currentX < order.Location.X)
            {
                currentX++;
            }
            else if (currentX > order.Location.X)
            {
                currentX--;
            }
            else if (currentY < order.Location.Y)
            {
                currentY++;
            }
            else if (currentY > order.Location.Y)
            {
                currentY--;
            }
            route.Add((currentX, currentY));
        }
        return route;
    }
}
