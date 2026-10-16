using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json;

namespace GreenhouseMonitoring.Infrastructure;

public static class SessionExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    // Запись объекта в сессию
    public static void Set<T>(this ISession session, string key, T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        session.SetString(key, json);
    }

    // Чтение объекта из сессии
    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value, JsonOptions);
    }

    // Запись словаря в сессию (явная перегрузка для Dictionary<string, string>)
    public static void Set(this ISession session, string key, Dictionary<string, string> dictionary)
    {
        var json = JsonSerializer.Serialize(dictionary, JsonOptions);
        session.SetString(key, json);
    }

    // Чтение словаря из сессии
    public static Dictionary<string, string>? Get(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<Dictionary<string, string>>(value, JsonOptions);
    }
}