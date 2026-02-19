using System.Net;
using System.Net.Http.Json;
using DriverFinder.WebApp.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace DriverFinder.WebApp.Tests;

[TestFixture]
public class IntegrationTests
{
    private WebApplicationFactory<Program> factory;
    private HttpClient client;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        factory = new WebApplicationFactory<Program>();
        client = factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task AddUpdateDriver_ValidCoordinates_ReturnsSuccessMessage()
    {
        // 1. Добавление водителя
        var driverDto = new DriverDto { Id = 1, X = 10, Y = 10 };
        var response = await client.PostAsJsonAsync("/Driver", driverDto);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Is.EqualTo("Координаты успешно добавлены"));

        // 2. Обновление координат того же водителя
        driverDto.X = 11;
        response = await client.PostAsJsonAsync("/Driver", driverDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Is.EqualTo("Координаты успешно изменены"));
    }

    [Test]
    public async Task AddDriver_InvalidCoordinates_ReturnsBadRequest()
    {
        // Выход за границы 100x100
        var driverDto = new DriverDto { Id = 2, X = -1, Y = 10 };
        var response = await client.PostAsJsonAsync("/Driver", driverDto);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Is.EqualTo("Координаты некорректны"));
    }

    [Test]
    public async Task AddMultipleDriversToSameCoordinates_ShouldBeAllowed()
    {
        // Размещаем водителя 3 на (20, 20)
        var driver3 = new DriverDto { Id = 3, X = 20, Y = 20 };
        var response3 = await client.PostAsJsonAsync("/Driver", driver3);
        Assert.That(response3.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Пытаемся разместить водителя 4 на те же координаты (20, 20)
        var driver4 = new DriverDto { Id = 4, X = 20, Y = 20 };
        var response4 = await client.PostAsJsonAsync("/Driver", driver4);
        Assert.That(response4.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var message4 = await response4.Content.ReadAsStringAsync();
        Assert.That(message4, Is.EqualTo("Координаты успешно добавлены"));
    }

    [Test]
    public async Task UpdateDriver_MovingToInvalidCoordinates_RemovesDriver()
    {
        // 1. Добавляем водителя 
        var driver5 = new DriverDto { Id = 5, X = 30, Y = 30 };
        await client.PostAsJsonAsync("/Driver", driver5);

        // 2. Обновляем координаты на некорректные
        driver5.X = 200; 
        var response = await client.PostAsJsonAsync("/Driver", driver5);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        // 3. Проверяем, что теперь можно добавить другого водителя на (30, 30)
        // Если водитель 5 удален, то (30, 30) свободны
        var driver6 = new DriverDto { Id = 6, X = 30, Y = 30 };
        var response6 = await client.PostAsJsonAsync("/Driver", driver6);
        Assert.That(response6.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var message6 = await response6.Content.ReadAsStringAsync();
        Assert.That(message6, Is.EqualTo("Координаты успешно добавлены"));
    }

    [Test]
    public async Task FindDriver_ValidOrder_ReturnsDriverDetails()
    {
        // 1. Убеждаемся, что водитель существует
        var driver = new DriverDto { Id = 10, X = 50, Y = 50 };
        await client.PostAsJsonAsync("/Driver", driver);

        // 2. Ищем водителя для заказа на (52, 53)
        var orderDto = new OrderDto { Id = 100, X = 52, Y = 53 };
        var response = await client.PostAsJsonAsync("/Order", orderDto);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var result = await response.Content.ReadFromJsonAsync<DriverResponse>();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DriverId, Is.EqualTo(10));
        Assert.That(result.Distance, Is.EqualTo(5));
        Assert.That(result.Route, Is.Not.Empty);
    }

    [Test]
    public async Task FindDriver_InvalidCoordinates_ReturnsBadRequest()
    {
        var orderDto = new OrderDto { Id = 101, X = -5, Y = 10 };
        var response = await client.PostAsJsonAsync("/Order", orderDto);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Is.EqualTo("Координаты некорректны"));
    }

    [Test]
    public async Task FindDriver_NoDrivers_ReturnsBadRequest()
    {
        // Используем новую фабрику, чтобы убедиться, что список водителей пуст
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var orderDto = new OrderDto { Id = 102, X = 10, Y = 10 };
        var response = await client.PostAsJsonAsync("/Order", orderDto);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var message = await response.Content.ReadAsStringAsync();
        Assert.That(message, Is.EqualTo("Свободных водителей нет"));
    }

    private class DriverResponse
    {
        public int DriverId { get; init; }
        public Point DriverCoordinates { get; init; }
        public int Distance { get; init; }
        public List<Point> Route { get; init; }
    }

    private class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}


