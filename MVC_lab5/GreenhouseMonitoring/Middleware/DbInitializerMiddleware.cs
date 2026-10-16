// GreenhouseMonitoring/Middleware/DbInitializerMiddleware.cs
using GreenhouseMonitoring.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace GreenhouseMonitoring.Middleware;

public class DbInitializerMiddleware
{
    private readonly RequestDelegate _next;

    public DbInitializerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (!context.Session.Keys.Contains("db_initialized"))
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await DbInitializer.InitializeAsync(dbContext, serviceProvider);
            context.Session.SetString("db_initialized", "true");
        }

        await _next(context);
    }
}

public static class DbInitializerExtensions
{
    public static IApplicationBuilder UseDbInitializer(this IApplicationBuilder builder)
        => builder.UseMiddleware<DbInitializerMiddleware>();
}