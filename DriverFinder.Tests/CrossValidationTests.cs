using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;
using NUnit.Framework;

namespace DriverFinder.Tests;

[TestFixture]
public class CrossValidationTests
{
    [Test]
    [Repeat(50)]
    public void AllFinders_ReturnTheSameResult_OnRandomData()
    {
        var random = new Random();
        var drivers = new List<Driver>();
        for (var i = 0; i < 1000; i++)
        {
            drivers.Add(new Driver(i, new Point(random.Next(-5000, 5000), random.Next(-5000, 5000))));
        }
        var order = new Order(new Point(random.Next(-5000, 5000), random.Next(-5000, 5000)));
        const int countToFind = 5;

        // Эталонный результат SimpleDriverFinder
        var simpleFinder = new SimpleDriverFinder();
        var expectedResult = simpleFinder.FindNearest(order, new List<Driver>(drivers), countToFind);
        var expectedIds = expectedResult.Select(d => d.Id).OrderBy(id => id).ToList();
        
        Assert.AreEqual(countToFind, expectedIds.Count, "Эталонный результат не содержит нужного количества водителей.");

        // ManualInsertionDriverFinder
        var manualFinder = new ManualInsertionDriverFinder();
        var manualResult = manualFinder.FindNearest(order, new List<Driver>(drivers), countToFind);
        var manualIds = manualResult.Select(d => d.Id).OrderBy(id => id).ToList();
        CollectionAssert.AreEqual(expectedIds, manualIds, "ManualInsertionDriverFinder дал неверный результат.");

        // PriorityQueueDriverFinder
        var pqFinder = new PriorityQueueDriverFinder();
        var pqResult = pqFinder.FindNearest(order, new List<Driver>(drivers), countToFind);
        var pqIds = pqResult.Select(d => d.Id).OrderBy(id => id).ToList();
        CollectionAssert.AreEqual(expectedIds, pqIds, "PriorityQueueDriverFinder дал неверный результат.");

        // PlinqDriverFinder
        var plinqFinder = new PlinqDriverFinder();
        var plinqResult = plinqFinder.FindNearest(order, new List<Driver>(drivers), countToFind);
        var plinqIds = plinqResult.Select(d => d.Id).OrderBy(id => id).ToList();
        CollectionAssert.AreEqual(expectedIds, plinqIds, "PlinqDriverFinder дал неверный результат.");

        // StatefulGridDriverFinder
        var gridFinder = new StatefulGridDriverFinder(gridCellSize: 500);
        foreach (var driver in drivers)
        {
            gridFinder.AddDriver(driver);
        }
        var gridResult = gridFinder.FindNearest(order, countToFind);
        var gridIds = gridResult.Select(d => d.Id).OrderBy(id => id).ToList();
        CollectionAssert.AreEqual(expectedIds, gridIds, "StatefulGridDriverFinder дал неверный результат.");
    }
}
