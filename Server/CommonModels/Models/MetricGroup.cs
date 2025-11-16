using System.Collections.Generic;
using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Группа метрик.
/// </summary>
public class MetricGroup : IHasId
{
  /// <summary>
  /// Id.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Имя группы
  /// </summary>
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Метрики группы.
  /// </summary>
  public virtual IList<MetricType> MetricTypes { get; set; }
}