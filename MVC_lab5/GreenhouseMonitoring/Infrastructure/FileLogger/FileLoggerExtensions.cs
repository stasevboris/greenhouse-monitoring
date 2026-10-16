using Microsoft.Extensions.Logging;

namespace GreenhouseMonitoring.Infrastructure;

public static class FileLoggerExtensions
{
    /// <summary>
    /// Добавляет провайдер файлового логгера в фабрику логгеров.
    /// </summary>
    public static ILoggerFactory AddFile(this ILoggerFactory factory, string filePath)
    {
        factory.AddProvider(new FileLoggerProvider(filePath));
        return factory;
    }
}