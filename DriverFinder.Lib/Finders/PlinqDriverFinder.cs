using DriverFinder.Lib.Models;

namespace DriverFinder.Lib.Finders;

/// <summary>
/// Использует PLINQ (Parallel LINQ) для распараллеливания поиска.
/// </summary>
public class PlinqDriverFinder : BaseDriverFinder
{
    public override List<Driver> FindNearest(Order order, List<Driver> drivers, int count)
    {
        if (count <= 0 || !drivers.Any())
            return [];
        
        return drivers
            .AsParallel()
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .OrderBy(driver => GetDistance(driver.Location, order.Location))
            .ThenBy(driver => driver.Id) 
            .Take(count)
            .ToList();
    }
}
