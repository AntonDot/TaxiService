namespace DriverFinder.Lib.Models;

public class Order
{
    public Point Location { get; }

    public Order(Point location)
    {
        Location = location;
    }
}