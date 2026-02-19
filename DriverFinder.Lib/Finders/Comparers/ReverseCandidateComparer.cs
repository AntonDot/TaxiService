namespace DriverFinder.Lib.Finders.Comparers;

/// <summary>
/// Вспомогательный класс для сравнения, чтобы в PriorityQueue наверху был элемент
/// с максимальным расстоянием.
/// </summary>
internal class ReverseCandidateComparer : IComparer<(double Distance, int Id)>
{
    public int Compare((double Distance, int Id) x, (double Distance, int Id) y)
    {
        var distanceCompare = y.Distance.CompareTo(x.Distance);
        if (distanceCompare != 0) return distanceCompare;
        return y.Id.CompareTo(x.Id); 
    }
}