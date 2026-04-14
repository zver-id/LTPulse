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
  
  /// <summary>
  /// Наименование графика в отчете.
  /// </summary>
  public virtual string NameOfChart { get; set;}
  
  /// <summary>
  /// Тип графика в отчете.
  /// </summary>
  public virtual string ChartType { get; set; }
}