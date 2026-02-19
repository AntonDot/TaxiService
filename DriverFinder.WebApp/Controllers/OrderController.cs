using DriverFinder.Lib.Models;
using DriverFinder.WebApp.Exceptions;
using DriverFinder.WebApp.Models;
using DriverFinder.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Point = DriverFinder.Lib.Models.Point;

namespace DriverFinder.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> FindDriver([FromBody] OrderDto orderDto)
    {
        try
        {
            var order = new Order(new Point(orderDto.X, orderDto.Y));
            var (driver, distance, route) = await orderService.FindDriverForOrder(order);
            return Ok(new { DriverId = driver.Id, DriverCoordinates = new { driver.Location.X, driver.Location.Y }, Distance = distance, Route = route });
        }
        catch (InvalidCoordinatesException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (NoDriversAvailableException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
