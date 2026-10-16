using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace GreenhouseMonitoring.Infrastructure;

public class FileLogger(string path) : ILogger
{
    private readonly string _filePath = path;
    private readonly object _lock = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // Включаем все уровни логирования (можно настроить по желанию)
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (formatter == null)
            return;

        string message = formatter(state, exception);
        if (string.IsNullOrEmpty(message))
            return;

        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}";
        if (exception != null)
            logEntry += Environment.NewLine + exception;

        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                File.AppendAllText(_filePath, logEntry + Environment.NewLine);
            }
        }
        catch
        {
            // Игнорируем ошибки записи в файл, чтобы не сломать приложение
        }
    }
}