using DriverFinder.Lib.Models;

namespace DriverFinder.Lib.Finders;
/// <summary>
/// Абстрактный базовый класс для всех алгоритмов поиска водителей.
/// Предоставляет общую реализацию для вычисления расстояния.
/// </summary>
public abstract class BaseDriverFinder
{
    /// <summary>
    /// Метод ищет пять ближайщих водителей к заказу.
    /// При равенстве расстояний приоритет у водителя с меньшим Id.
    /// </summary>
    public abstract List<Driver> FindNearest(Order order, List<Driver> drivers, int count);
    
    /// <summary>
    /// Вычисляет расстояние между двумя точками.
    /// Движение только по двум осям X, Y.
    /// </summary>
    protected static int GetDistance(Point p1, Point p2)
    {
        return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
    }
}