using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Common;

public static class Settings
{
  /// <summary>
  /// Строка подключения к базе данных.
  /// </summary>
  public static readonly string DatabaseConnectionString;
  
  /// <summary>
  /// Строка подключения RabbitMQ.
  /// </summary>
  public static readonly string RabbitMQConnectionString;

  /// <summary>
  /// Конструктор.
  /// </summary>
  static Settings()
  {
    var settingsJson = File.ReadAllText("settings.json");
    dynamic settings = JsonConvert.DeserializeObject(settingsJson)!;
    var type = typeof(Settings);
    foreach (var setting in settings)
    {
      var fieldName = setting.Name;
      var fieldValue = setting.Value.ToString();
      FieldInfo fieldInfo = type.GetField(fieldName,
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      if (fieldInfo != null)
      {
        object value = Convert.ChangeType(fieldValue, fieldInfo.FieldType);
        fieldInfo.SetValue(null, value);
      }
    }
  }
}

