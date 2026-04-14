using CommonModels.Interfaces;

namespace CommonModels.Models;

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
  /// Группа метрики.
  /// </summary>
  public virtual MetricGroup MetricGroup { get; set; }
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  public MetricType() { }
}