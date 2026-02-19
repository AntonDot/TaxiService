using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;
using NUnit.Framework;

namespace DriverFinder.Lib.Tests;

[TestFixture]
public class StatefulGridDriverFinderTests
{
    [Test]
    public void FindNearest_AfterAddingDrivers_FindsCorrectDrivers()
    {
        var finder = new StatefulGridDriverFinder(gridCellSize: 10);
        var order = new Order(new Point(0, 0));
        
        finder.AddDriver(new Driver(1, new Point(1, 1)));
        finder.AddDriver(new Driver(2, new Point(2, 2)));
        finder.AddDriver(new Driver(3, new Point(3, 3)));
        finder.AddDriver(new Driver(4, new Point(4, 4)));
        finder.AddDriver(new Driver(5, new Point(5, 5)));
        finder.AddDriver(new Driver(6, new Point(60, 60)));
        finder.AddDriver(new Driver(7, new Point(70, 70)));

        var nearest = finder.FindNearest(order, 5);

        Assert.AreEqual(5, nearest.Count);
        var nearestIds = nearest.Select(d => d.Id).OrderBy(id => id).ToList();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, nearestIds);
    }

    [Test]
    public void FindNearest_AfterUpdatingDriver_ExcludesMovedDriver()
    {
        var finder = new StatefulGridDriverFinder(gridCellSize: 10);
        var order = new Order(new Point(0, 0));
        
        var driverToMove = new Driver(1, new Point(1, 1));
        finder.AddDriver(driverToMove);
        finder.AddDriver(new Driver(2, new Point(2, 2)));
        finder.AddDriver(new Driver(3, new Point(3, 3)));
        finder.AddDriver(new Driver(4, new Point(4, 4)));
        finder.AddDriver(new Driver(5, new Point(5, 5)));
        finder.AddDriver(new Driver(6, new Point(6, 6)));
        
        finder.UpdateDriverLocation(driverToMove, new Point(100, 100));
        var nearest = finder.FindNearest(order, 5);

        Assert.AreEqual(5, nearest.Count);
        var nearestIds = nearest.Select(d => d.Id).OrderBy(id => id).ToList();
        CollectionAssert.AreEqual(new[] { 2, 3, 4, 5, 6 }, nearestIds);
        CollectionAssert.DoesNotContain(nearestIds, 1);
    }
}