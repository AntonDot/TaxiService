namespace DriverFinder.Lib.Models;

public class Driver(int id, Point location)
{
    public int Id { get; } = id;
    public Point Location { get; set; } = location;
}