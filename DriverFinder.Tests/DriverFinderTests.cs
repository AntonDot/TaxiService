using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;
using NUnit.Framework;

namespace DriverFinder.Tests;

[TestFixture]
public class DriverFinderTests
{
    private List<Driver> _drivers = null!;
    private Order _order = null!;

    [SetUp]
    public void Setup()
    {
        _drivers = new List<Driver>
        {
            new(1, new Point(1, 1)),
            new(2, new Point(2, 2)),
            new(3, new Point(3, 3)),
            new(4, new Point(4, 4)),
            new(5, new Point(5, 5)),
            new(6, new Point(60, 60)),
            new(7, new Point(70, 70)),
            new(8, new Point(-80, 80)),
            new(9, new Point(9, 9)),
            new(10, new Point(10, 10))
        };
        _order = new Order(new Point(0, 0));
    }
    
    private static void AssertFindsCorrectDrivers(List<Driver> nearest)
    {
        Assert.AreEqual(5, nearest.Count);
        var nearestIds = nearest.Select(d => d.Id).OrderBy(id => id).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, nearestIds);
    }

    [Test]
    public void SimpleDriverFinder_FindsNearestDrivers()
    {
        BaseDriverFinder finder = new SimpleDriverFinder();
        var nearest = finder.FindNearest(_order, new List<Driver>(_drivers), 5);
        AssertFindsCorrectDrivers(nearest);
    }

    [Test]
    public void ManualInsertionDriverFinder_FindsNearestDrivers()
    {
        BaseDriverFinder finder = new ManualInsertionDriverFinder();
        var nearest = finder.FindNearest(_order, new List<Driver>(_drivers), 5);
        AssertFindsCorrectDrivers(nearest);
    }

    [Test]
    public void PriorityQueueDriverFinder_FindsNearestDrivers()
    {
        BaseDriverFinder finder = new PriorityQueueDriverFinder();
        var nearest = finder.FindNearest(_order, new List<Driver>(_drivers), 5);
        AssertFindsCorrectDrivers(nearest);
    }
    
    [Test]
    public void PlinqDriverFinder_FindsNearestDrivers()
    {
        BaseDriverFinder finder = new PlinqDriverFinder();
        var nearest = finder.FindNearest(_order, new List<Driver>(_drivers), 5);
        AssertFindsCorrectDrivers(nearest);
    }
}