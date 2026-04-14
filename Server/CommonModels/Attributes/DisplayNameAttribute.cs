using System;

namespace CommonModels.Attributes;

/// <summary>
/// Атрибут отображаемого имени.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DisplayNameAttribute : Attribute
{
  /// <summary>
  /// Отображаемое имя.
  /// </summary>
  public string DisplayName { get; set; }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="displayName">Отображаемое имя.</param>
  public DisplayNameAttribute(string displayName)
  {
    this.DisplayName = displayName;
  }
}