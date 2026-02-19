using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;
using DriverFinder.WebApp.Exceptions;
using DriverFinder.WebApp.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Point = DriverFinder.Lib.Models.Point;

namespace DriverFinder.WebApp.Services;

public class DriverService(
    IOptions<MapSettings> mapSettings,
    StatefulGridDriverFinder driverFinder,
    ILogger<DriverService> logger)
{
    private readonly MapSettings mapSettings = mapSettings.Value;
    private readonly Dictionary<int, Driver> drivers = new();

    public string AddOrUpdateDriver(int id, int x, int y)
    {
        if (x < 0 || x >= mapSettings.N || y < 0 || y >= mapSettings.M)
        {
            if (drivers.TryGetValue(id, out var driverToRemove))
            {
                logger.LogWarning("Водитель {Id} удален из-за некорректных новых координат: ({X}, {Y})", id, x, y);
                driverFinder.RemoveDriver(driverToRemove);
                drivers.Remove(id);
            }
            else
            {
                logger.LogWarning("Попытка добавления водителя {Id} с некорректными координатами: ({X}, {Y})", id, x, y);
            }
            throw new InvalidCoordinatesException("Координаты некорректны");
        }

        if (drivers.Any(d => d.Value.Location.X == x && d.Value.Location.Y == y && d.Key != id))
        {
            logger.LogWarning("Координаты ({X}, {Y}) уже заняты другим водителем. Попытка для водителя {Id}", x, y, id);
            throw new CoordinatesOccupiedException("Здесь уже находится другой водитель");
        }

        if (drivers.TryGetValue(id, out var driverToUpdate))
        {
            driverFinder.UpdateDriverLocation(driverToUpdate, new Point(x, y));
            logger.LogInformation("Координаты водителя {Id} успешно изменены на ({X}, {Y})", id, x, y);
            return "Координаты успешно изменены";
        }

        var newDriver = new Driver(id, new Point(x, y));
        drivers.Add(id, newDriver);
        driverFinder.AddDriver(newDriver);
        logger.LogInformation("Водитель {Id} успешно добавлен с координатами ({X}, {Y})", id, x, y);
        return "Координаты успешно добавлены";
    }

    public List<Driver> GetDrivers()
    {
        return drivers.Values.ToList();
    }
}
