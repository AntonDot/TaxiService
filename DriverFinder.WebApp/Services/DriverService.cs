using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;
using DriverFinder.WebApp.Exceptions;
using DriverFinder.WebApp.Settings;
using Microsoft.Extensions.Options;
using Point = DriverFinder.Lib.Models.Point;

namespace DriverFinder.WebApp.Services;

public class DriverService(IOptions<MapSettings> mapSettings, StatefulGridDriverFinder driverFinder)
{
    private readonly MapSettings mapSettings = mapSettings.Value;
    private readonly Dictionary<int, Driver> drivers = new();

    public string AddOrUpdateDriver(int id, int x, int y)
    {
        if (x < 0 || x >= mapSettings.N || y < 0 || y >= mapSettings.M)
        {
            if (drivers.TryGetValue(id, out var driverToRemove))
            {
                driverFinder.RemoveDriver(driverToRemove);
                drivers.Remove(id);
            }
            throw new InvalidCoordinatesException("Координаты некорректны");
        }

        if (drivers.Any(d => d.Value.Location.X == x && d.Value.Location.Y == y && d.Key != id))
        {
            throw new CoordinatesOccupiedException("Здесь уже находится другой водитель");
        }

        if (drivers.TryGetValue(id, out var driverToUpdate))
        {
            driverFinder.UpdateDriverLocation(driverToUpdate, new Point(x, y));
            return "Координаты успешно изменены";
        }

        var newDriver = new Driver(id, new Point(x, y));
        drivers.Add(id, newDriver);
        driverFinder.AddDriver(newDriver);
        return "Координаты успешно добавлены";
    }

    public List<Driver> GetDrivers()
    {
        return drivers.Values.ToList();
    }
}
