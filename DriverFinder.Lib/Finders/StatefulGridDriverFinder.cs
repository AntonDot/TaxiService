using DriverFinder.Lib.Finders.Comparers;
using DriverFinder.Lib.Models;

namespace DriverFinder.Lib.Finders;

/// <summary>
///Поиск с помощью более крупной сетки
/// </summary>
public class StatefulGridDriverFinder(int gridCellSize = 100)
{
    private readonly Dictionary<Point, List<Driver>> grid = new();
    private readonly Dictionary<int, Point> driverLocations = new();

    public void AddDriver(Driver driver)
    {
        var location = driver.Location;
        var cell = GetCell(location);
        if (!grid.TryGetValue(cell, out var driversInCell))
        {
            driversInCell = [];
            grid[cell] = driversInCell;
        }
        driversInCell.Add(driver);
        driverLocations[driver.Id] = location;
    }

    public void RemoveDriver(Driver driver)
    {
        if (!driverLocations.TryGetValue(driver.Id, out var location)) return;
        var cell = GetCell(location);
        if (grid.TryGetValue(cell, out var driversInCell))
        {
            driversInCell.RemoveAll(d => d.Id == driver.Id);
            if (driversInCell.Count == 0) grid.Remove(cell);
        }
        driverLocations.Remove(driver.Id);
    }

    public void UpdateDriverLocation(Driver driver, Point newLocation)
    {
        RemoveDriver(driver);
        driver.Location = newLocation;
        AddDriver(driver);
    }

    public List<Driver> FindNearest(Order order, int count)
    {
        if (count <= 0 || driverLocations.Count == 0)
            return [];

        var orderCell = GetCell(order.Location);
        var searchRadius = 0;
        var processedDrivers = new HashSet<int>();

        var comparer = new ReverseCandidateComparer();
        var bestCandidates = new PriorityQueue<Driver, (double Distance, int Id)>(comparer);

        while (true)
        {
            var layerCells = GetCellsInLayer(orderCell, searchRadius).Distinct();
            
            foreach (var cell in layerCells)
            {
                if (!grid.TryGetValue(cell, out var driversInCell)) continue;
                foreach (var driver in driversInCell)
                {
                    if (!processedDrivers.Add(driver.Id)) continue;
                    var priority = (GetDistance(driver.Location, order.Location), driver.Id);
                    if (bestCandidates.Count < count)
                        bestCandidates.Enqueue(driver, priority);
                    else
                    {
                        bestCandidates.TryPeek(out _, out var topPriority);
                        if (comparer.Compare(priority, topPriority) > 0)
                            bestCandidates.DequeueEnqueue(driver, priority);
                        
                    }
                }
            }

            if (bestCandidates.Count >= count)
            {
                bestCandidates.TryPeek(out _, out var furthestCandidatePriority);
                double minDistanceToNextLayer = searchRadius * gridCellSize;

                if (furthestCandidatePriority.Distance < minDistanceToNextLayer)
                {
                    break; 
                }
            }

            if (processedDrivers.Count == driverLocations.Count)
            {
                break;
            }

            searchRadius++;
        }

        var result = new List<Driver>();
        while (bestCandidates.TryDequeue(out var driver, out _))
        {
            result.Add(driver);
        }
        
        result.Sort(new DriverDistanceComparer(order));

        return result;
    }

    private Point GetCell(Point location) => new(location.X / gridCellSize, location.Y / gridCellSize);
    private static double GetDistance(Point p1, Point p2) => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);

    private static IEnumerable<Point> GetCellsInLayer(Point centerCell, int radius)
    {
        if (radius == 0)
        {
            yield return centerCell;
            yield break;
        }
        for (var i = -radius; i <= radius; i++)
        {
            yield return new Point(centerCell.X + i, centerCell.Y + radius);
            yield return new Point(centerCell.X + i, centerCell.Y - radius);
        }
        for (var i = -radius + 1; i < radius; i++)
        {
            yield return new Point(centerCell.X + radius, centerCell.Y + i);
            yield return new Point(centerCell.X - radius, centerCell.Y + i);
        }
    }
}
