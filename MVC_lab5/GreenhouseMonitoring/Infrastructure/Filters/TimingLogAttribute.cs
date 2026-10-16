using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GreenhouseMonitoring.Infrastructure.Filters;

public class TimingLogAttribute(ILoggerFactory loggerFactory) : Attribute, IResourceFilter
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimingLogAttribute>();

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        _logger.LogInformation("OnResourceExecuting - {Time}, Path: {Path}",
            DateTime.Now, context.HttpContext.Request.Path);
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        _logger.LogInformation("OnResourceExecuted - {Time}, Path: {Path}",
            DateTime.Now, context.HttpContext.Request.Path);
    }
}