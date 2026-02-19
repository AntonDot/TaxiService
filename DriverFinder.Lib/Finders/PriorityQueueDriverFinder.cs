using DriverFinder.Lib.Finders.Comparers;
using DriverFinder.Lib.Models;

namespace DriverFinder.Lib.Finders;

/// <summary>
/// Находит ближайших водителей с использованием PriorityQueue.
/// </summary>
public class PriorityQueueDriverFinder : BaseDriverFinder
{
    public override List<Driver> FindNearest(Order order, List<Driver> drivers, int count)
    {
        if (count <= 0 || !drivers.Any())
            return [];
        
        var comparer = new ReverseCandidateComparer();
        var priorityQueue = new PriorityQueue<Driver, (double Distance, int Id)>(comparer);

        foreach (var driver in drivers)
        {
            var newPriority = (GetDistance(driver.Location, order.Location), driver.Id);

            if (priorityQueue.Count < count)
                priorityQueue.Enqueue(driver, newPriority);
            else
            {
                priorityQueue.TryPeek(out _, out var topPriority);
                if (comparer.Compare(newPriority, topPriority) > 0)
                    priorityQueue.DequeueEnqueue(driver, newPriority);
            }
        }

        var result = new List<Driver>();
        while (priorityQueue.TryDequeue(out var driver, out _))
        {
            result.Add(driver);
        }
        
        result.Sort(new DriverDistanceComparer(order));

        return result;
    }
}
