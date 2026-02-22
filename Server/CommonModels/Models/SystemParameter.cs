using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Параметр системы.
/// </summary>
public class SystemParameter : IHasId
{
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Наименование.
  /// </summary>
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Значение.
  /// </summary>
  public virtual string Value { get; set; }
}