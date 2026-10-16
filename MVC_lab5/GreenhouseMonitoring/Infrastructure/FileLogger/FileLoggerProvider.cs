using Microsoft.Extensions.Logging;

namespace GreenhouseMonitoring.Infrastructure;

public class FileLoggerProvider(string path) : ILoggerProvider
{
    private readonly string _path = path;

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_path);
    }

    public void Dispose()
    {
        // Нет управляемых ресурсов для освобождения
    }
}