using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;

namespace GreenhouseMonitoring.Infrastructure.Filters;


public class CacheResourceFilterAttribute : Attribute, IResourceFilter
{
    // Используем ConcurrentDictionary для потокобезопасности
    private static readonly ConcurrentDictionary<string, object> _cache = new();
    private string? _cacheKey;

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        _cacheKey = context.HttpContext.Request.Path.ToString();

        if (_cache.TryGetValue(_cacheKey, out var value) && value is ViewResult cachedResult)
        {
            context.Result = cachedResult;
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        if (!string.IsNullOrEmpty(_cacheKey) &&
            context.Result is ViewResult result &&
            !_cache.ContainsKey(_cacheKey))
        {
            _cache.TryAdd(_cacheKey, result);
        }
    }
}