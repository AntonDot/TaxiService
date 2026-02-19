using DriverFinder.Lib.Models;

namespace DriverFinder.Lib.Finders;

/// <summary>
/// Находит ближайших водителей, поддерживая небольшой отсортированный список кандидатов.
/// </summary>
public class ManualInsertionDriverFinder : BaseDriverFinder
{
    private readonly record struct Candidate(Driver Driver, int Distance, int Id);

    public override List<Driver> FindNearest(Order order, List<Driver> drivers, int count)
    {
        if (count <= 0 || !drivers.Any())
        {
            return new List<Driver>();
        }

        var nearestCandidates = new List<Candidate>();
        var comparer = Comparer<Candidate>.Create((a, b) =>
        {
            var distanceCompare = a.Distance.CompareTo(b.Distance);
            if (distanceCompare != 0) return distanceCompare;
            return a.Id.CompareTo(b.Id); 
        });

        foreach (var driver in drivers)
        {
            var distance = GetDistance(driver.Location, order.Location);
            var newCandidate = new Candidate(driver, distance, driver.Id);

            if (nearestCandidates.Count < count)
            {
                nearestCandidates.Add(newCandidate);
                nearestCandidates.Sort(comparer);
            }
            // Сравниваем с самым дальним кандидатом
            else if (comparer.Compare(newCandidate, nearestCandidates[count - 1]) < 0)
            {
                // Новый кандидат лучше самого дальнего
                nearestCandidates[count - 1] = newCandidate;
                nearestCandidates.Sort(comparer);
            }
        }

        return nearestCandidates.Select(c => c.Driver).ToList();
    }
}
