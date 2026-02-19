using DriverFinder.WebApp.Exceptions;
using DriverFinder.WebApp.Models;
using DriverFinder.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace DriverFinder.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class DriverController(DriverService driverService) : ControllerBase
{
    [HttpPost]
    public IActionResult AddOrUpdateDriver([FromBody] DriverDto driver)
    {
        try
        {
            var result = driverService.AddOrUpdateDriver(driver.Id, driver.X, driver.Y);
            return Ok(result);
        }
        catch (InvalidCoordinatesException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
