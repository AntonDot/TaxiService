using BenchmarkDotNet.Attributes;
using DriverFinder.Lib.Finders;
using DriverFinder.Lib.Models;

namespace DriverFinder.App;

[MemoryDiagnoser]
[RankColumn]
public class DriverFinderBenchmark
{
    private List<Driver> _drivers = null!;
    private readonly Order _order = new(new Point(0, 0));
    private const int FindCount = 50;

    private readonly SimpleDriverFinder _simpleFinder = new();
    private readonly ManualInsertionDriverFinder _manualFinder = new(); 
    private readonly PriorityQueueDriverFinder _pqFinder = new();
    private readonly PlinqDriverFinder _plinqFinder = new();
    private StatefulGridDriverFinder _statefulGridFinder = null!;

    [Params(1000, 10000, 50000, 100000)]
    public int DriversCount;

    [GlobalSetup]
    public void Setup()
    {
        _statefulGridFinder = new StatefulGridDriverFinder(gridCellSize: 1000);
        _drivers = new List<Driver>(DriversCount);
        var random = new Random(42);
        for (var i = 0; i < DriversCount; i++)
        {
            var driver = new Driver(i, new Point(random.Next(-100000, 100000), random.Next(-100000, 100000)));
            _drivers.Add(driver);
        }
        
        foreach (var driver in _drivers)
        {
            _statefulGridFinder.AddDriver(driver);
        }
    }

    [Benchmark(Baseline = true)]
    public void SimpleSort_Finder()
    {
        _simpleFinder.FindNearest(_order, _drivers, FindCount);
    }

    [Benchmark]
    public void ManualInsertion_Finder() 
    {
        _manualFinder.FindNearest(_order, _drivers, FindCount);
    }

    [Benchmark]
    public void PriorityQueue_Finder()
    {
        _pqFinder.FindNearest(_order, _drivers, FindCount);
    }

    [Benchmark]
    public void Plinq_Finder()
    {
        _plinqFinder.FindNearest(_order, _drivers, FindCount);
    }

    [Benchmark]
    public void StatefulGrid_Finder()
    {
        _statefulGridFinder.FindNearest(_order, FindCount);
    }
}
