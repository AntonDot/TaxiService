using DriverFinder.Lib.Models;

namespace DriverFinder.Lib.Finders;

public class SimpleDriverFinder : BaseDriverFinder
{
    public override List<Driver> FindNearest(Order order, List<Driver> drivers, int count)
    {
        return drivers
            .OrderBy(driver => GetDistance(driver.Location, order.Location))
            .ThenBy(driver => driver.Id) 
            .Take(count)
            .ToList();
    }
}
