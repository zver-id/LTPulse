using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommonModels.Attributes;

public static class DisplayNameGetter
{
  /// <summary>
  /// Получить значение атрибута для свойства.
  /// </summary>
  /// <param name="propertyExpression">Свойство.</param>
  /// <typeparam name="T">Тип объекта.</typeparam>
  /// <returns>Значение атрибута.</returns>
  public static string? GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
  {
    var memberExpression = propertyExpression.Body as MemberExpression 
                           ?? ((UnaryExpression)propertyExpression.Body).Operand as MemberExpression;
    var propertyInfo = memberExpression?.Member as PropertyInfo;
    var attribute = propertyInfo?.GetCustomAttribute<DisplayNameAttribute>();
    return attribute?.DisplayName;
  }
  
  /// <summary>
  /// Получить все атрибуты с их отображаемым именем.
  /// </summary>
  /// <typeparam name="T">Тип модели.</typeparam>
  /// <returns>Свойства и их отображаемые имена.</returns>
  public static Dictionary<string, PropertyInfo> GetPropertiesWithDisplayNames<T>()
  {
    return typeof(T)
      .GetProperties()
      .Where(p => p.GetCustomAttribute<DisplayNameAttribute>() != null)
      .ToDictionary(
        p => p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName,
        p => p
      );
  }
}
