using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ActionEntity = GreenhouseMonitoring.Models.Action;

namespace GreenhouseMonitoring.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
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

        var reading = new SensorReading
        {
            SensorId = sensor.SensorId,
            Value = 25.5m,
            Timestamp = DateTime.Now
        };
        context.SensorReadings.Add(reading);
        await context.SaveChangesAsync();

        var ev = new Event
        {
            SensorId = sensor.SensorId,
            ReadingId = reading.ReadingId,
            Description = "Нормальные показания",
            Criticality = "Low",
            Timestamp = DateTime.Now
        };
        context.Events.Add(ev);
        await context.SaveChangesAsync();

        var action = new ActionEntity
        {
            EventId = ev.EventId,
            Type = "Проверка",
            Description = "Проверка датчика",
            Status = "Planned",
            Timestamp = DateTime.Now
        };
        context.Actions.Add(action);
        await context.SaveChangesAsync();

        var parameter = new NormativeParameter
        {
            CropId = crop.CropId,
            Parameter = "Temperature",
            MinValue = 20m,
            MaxValue = 30m,
            GrowthStage = "Вегетация"
        };
        context.NormativeParameters.Add(parameter);
        await context.SaveChangesAsync();

        // === Создаём пользователей и роли ===
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Administrator", "Manager", "User" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var adminEmail = "admin@example.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Administrator");
        }

        var userEmail = "user@example.com";
        if (await userManager.FindByEmailAsync(userEmail) == null)
        {
            var user = new IdentityUser { UserName = userEmail, Email = userEmail };
            await userManager.CreateAsync(user, "User123!");
            await userManager.AddToRoleAsync(user, "User");
        }
    }
}