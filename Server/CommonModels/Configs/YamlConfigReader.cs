using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace  CommonModels.Configs;

public class YamlConfigReader
{
  private readonly string _filePath;
  private readonly IDeserializer _deserializer;

  public YamlConfigReader(string filePath)
  {
    _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        
    _deserializer = new DeserializerBuilder()
      .WithNamingConvention(CamelCaseNamingConvention.Instance)
      .IgnoreUnmatchedProperties()
      .Build();
  }

  /// <summary>
  /// Читает конфигурацию из YAML файла и десериализует в указанный тип
  /// </summary>
  /// <typeparam name="T">Тип конфигурации</typeparam>
  /// <returns>Объект конфигурации</returns>
  public T ReadConfig<T>() where T : class, new()
  {
    if (!File.Exists(_filePath))
    {
      throw new FileNotFoundException($"YAML файл конфигурации не найден: {_filePath}");
    }

    try
    {
      var yamlContent = File.ReadAllText(_filePath);
      return _deserializer.Deserialize<T>(yamlContent) ?? new T();
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Ошибка при чтении YAML файла: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Проверяет существование файла конфигурации
  /// </summary>
  public bool ConfigFileExists()
  {
    return File.Exists(_filePath);
  }
}