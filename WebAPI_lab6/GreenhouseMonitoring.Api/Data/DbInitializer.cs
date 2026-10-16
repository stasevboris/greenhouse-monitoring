using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace GreenhouseMonitoring.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, IServiceProvider serviceProvider)
    {
        // Применяем миграции (если их нет — создадутся)
        await context.Database.MigrateAsync();

        // Создаём начальные данные, если их нет
        if (context.Crops.Any())
            return;

        // === Культуры и теплицы ===
        var crop = new Crop { Name = "Томат" };
        context.Crops.Add(crop);
        await context.SaveChangesAsync();

        var greenhouse = new Greenhouse
        {
            NameNumber = "Теплица №1",
            Location = "Ферма А",
            CropId = crop.CropId
        };
        context.Greenhouses.Add(greenhouse);
        await context.SaveChangesAsync();

        var sensor = new Sensor
        {
            Type = "Temperature",
            Model = "DHT22",
            LocationInGh = "Центр",
            GreenhouseId = greenhouse.GreenhouseId
        };
        context.Sensors.Add(sensor);
        await context.SaveChangesAsync();
    }
}