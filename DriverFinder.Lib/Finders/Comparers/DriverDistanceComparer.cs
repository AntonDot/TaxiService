using DriverFinder.Lib.Models;

namespace DriverFinder.Lib.Finders.Comparers;

/// <summary>
/// Сравнивает двух водителей на основе их расстояния до заданного заказа.
/// </summary>
public class DriverDistanceComparer(Order order) : IComparer<Driver>
{
    private readonly Point _orderLocation = order.Location;

    public int Compare(Driver? d1, Driver? d2)
    {
        if (d1 == null && d2 == null) return 0;
        if (d1 == null) return -1;
        if (d2 == null) return 1;

        var distance1 = GetDistance(d1.Location, _orderLocation);
        var distance2 = GetDistance(d2.Location, _orderLocation);

        var distanceCompare = distance1.CompareTo(distance2);
        if (distanceCompare != 0)
        {
            return distanceCompare;
        }
        
        return d1.Id.CompareTo(d2.Id);
    }

    private static double GetDistance(Point p1, Point p2)
    {
        return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
    }
}
