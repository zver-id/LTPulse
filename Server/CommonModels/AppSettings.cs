using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CommonModels;

public static class AppSettings
{
  public static readonly string DatabaseConnectionString;
  public static readonly string RabbitMQConnectionString;

  static AppSettings()
  {
    var settingsJson = File.ReadAllText("settings.json");
    dynamic settings = JsonConvert.DeserializeObject(settingsJson)!;
    var type = typeof(AppSettings);
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

