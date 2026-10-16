using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GreenhouseMonitoring.Infrastructure;

public static class Transformations
{
    public static T DictionaryToObject<T>(IDictionary<string, string> dict) where T : new()
    {
        var obj = new T();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Ищем ключ в словаре, игнорируя регистр
            var match = dict.FirstOrDefault(x =>
                string.Equals(x.Key, property.Name, StringComparison.OrdinalIgnoreCase));

            if (match.Value == null)
                continue;

            try
            {
                // Определяем целевой тип (с учётом Nullable)
                var propertyType = property.PropertyType;
                var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                // Преобразуем значение
                object? convertedValue = string.IsNullOrEmpty(match.Value)
                    ? null
                    : Convert.ChangeType(match.Value, targetType);

                // Устанавливаем значение свойства
                property.SetValue(obj, convertedValue);
            }
            catch (Exception)
            {
                // В случае ошибки (например, несовместимый тип) — пропускаем
                // Можно заменить на логирование при необходимости
            }
        }

        return obj;
    }
}