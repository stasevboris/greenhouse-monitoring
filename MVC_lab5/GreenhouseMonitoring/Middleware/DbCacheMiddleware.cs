using GreenhouseMonitoring.Services;
using GreenhouseMonitoring.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace GreenhouseMonitoring.Middleware;

public class DbCacheMiddleware(RequestDelegate next, IMemoryCache memoryCache)
{
    private readonly RequestDelegate _next = next;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private const string CacheKey = "HomeData";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(274); 

    public async Task InvokeAsync(HttpContext httpContext, IHomeService homeService)
    {
        // Пытаемся получить данные из кэша
        if (!_memoryCache.TryGetValue(CacheKey, out HomeViewModel? viewModel))
        {
            // Если нет — получаем через сервис
            viewModel = homeService.GetHomeViewModel();

            // Сохраняем в кэше на 274 секунд
            _memoryCache.Set(CacheKey, viewModel, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            });
        }

        // Передаём управление дальше (например, контроллеру)
        await _next(httpContext);
    }
}

// Метод расширения для удобного подключения middleware
public static class DbCacheMiddlewareExtensions
{
    public static IApplicationBuilder UseHomeDataCache(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DbCacheMiddleware>();
    }
}