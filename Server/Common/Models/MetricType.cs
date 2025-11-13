using Common.Interfaces;

namespace Common.Models;

/// <summary>
/// Тип метрики.
/// </summary>
public class MetricType : IHasId
{
  /// <summary>
  /// ID метрики.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Наименование метрики.
  /// </summary>
  public virtual string Name { get; set; }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="name">Имя.</param>
  public MetricType(string name)
  {
    this.Name = name;
  }
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  public MetricType() { }
}