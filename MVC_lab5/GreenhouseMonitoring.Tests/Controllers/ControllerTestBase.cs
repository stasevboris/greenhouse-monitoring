using GreenhouseMonitoring.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace GreenhouseMonitoring.Tests.Controllers;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<ControllerTestBase>
{
}

[Collection("Database")]
public class ControllerTestBase : IDisposable
{
    protected ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        // Ничего не делаем — InMemory сама очищается
    }
}