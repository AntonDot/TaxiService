using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;
using DriverFinder.WebApp.Exceptions;
using DriverFinder.WebApp.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Point = DriverFinder.Lib.Models.Point;

namespace DriverFinder.WebApp.Services;

public class OrderService(
    IOptions<MapSettings> mapSettings,
    DriverService driverService,
    StatefulGridDriverFinder driverFinder,
    ILogger<OrderService> logger)
{
    private readonly MapSettings mapSettings = mapSettings.Value;
    private readonly HttpClient httpClient = new();

    public async Task<(Driver? driver, int distance, List<Point>? route)> FindDriverForOrder(Order order)
    {
        if (order.Location.X < 0 || order.Location.X >= mapSettings.N || order.Location.Y < 0 || order.Location.Y >= mapSettings.M)
        {
            logger.LogWarning("Поступил заказ с некорректными координатами: ({X}, {Y})", order.Location.X, order.Location.Y);
            throw new InvalidCoordinatesException("Координаты некорректны");
        }

        if (driverService.GetDrivers().Count == 0)
        {
            logger.LogWarning("Заказ на ({X}, {Y}) отклонен: свободные водители отсутствуют на карте", order.Location.X, order.Location.Y);
            throw new NoDriversAvailableException("Свободных водителей нет");
        }

        var candidateDrivers = driverFinder.FindNearest(order, 10);
        if (candidateDrivers.Count == 0)
        {
            logger.LogWarning("Для заказа на ({X}, {Y}) не найдено ближайших водителей", order.Location.X, order.Location.Y);
            throw new NoDriversAvailableException("Не удалось найти подходящих водителей поблизости");
        }
        
        var bestDistance = Math.Abs(candidateDrivers.First().Location.X - order.Location.X) + Math.Abs(candidateDrivers.First().Location.Y - order.Location.Y);
        var bestDrivers = candidateDrivers.Where(d => Math.Abs(d.Location.X - order.Location.X) + Math.Abs(d.Location.Y - order.Location.Y) == bestDistance).ToList();

        int randomIndex;
        try
        {
            var response = await httpClient.GetStringAsync("http://www.randomnumberapi.com/api/v1.0/random?min=0&max=" + (bestDrivers.Count) + "&count=1");
            logger.LogInformation("Внешнее API вернуло: {Response}", response);
            randomIndex = int.Parse(response.Trim().Trim('[', ']'));
            logger.LogInformation("Для заказа на ({X}, {Y}) выбрано случайное число {Index} через внешнее API", order.Location.X, order.Location.Y, randomIndex);
        }
        catch (Exception ex)
        {
            randomIndex = new Random().Next(0, bestDrivers.Count);
            logger.LogWarning(ex, "Ошибка внешнего API для заказа на ({X}, {Y}). Используется локальный Random: {Index}", order.Location.X, order.Location.Y, randomIndex);
        }

        var selectedDriver = bestDrivers[randomIndex];
        var route = GetRoute(selectedDriver, order);
        logger.LogInformation("Для заказа на ({X}, {Y}) назначен водитель {DriverId} на расстоянии {Distance}", 
            order.Location.X, order.Location.Y, selectedDriver.Id, bestDistance);
        
        return (selectedDriver, bestDistance, route);
    }

    private List<Point> GetRoute(Driver driver, Order order)
    {
        var route = new List<Point>();
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
            route.Add(new Point(currentX, currentY));
        }
        return route;
    }
}
