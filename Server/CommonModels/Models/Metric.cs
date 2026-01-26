using System;
using System.Collections.Generic;
using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Метрика команды.
/// </summary>
public class Metric : IHasId
{
  /// <summary>
  /// ID метрики.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Дата метрики.
  /// </summary>
  public virtual DateTime Date { get; set; }
  
  /// <summary>
  /// Команда, которой принадлежит метрика.
  /// </summary>
  public virtual Team Team { get; set; }
  
  /// <summary>
  /// Тип метрики.
  /// </summary>
  public virtual MetricType MetricType { get; set; }
  
  /// <summary>
  /// Значение метрики.
  /// </summary>
  public virtual float Value { get; set; }

  /// <summary>
  /// Список обращений, связанных с метрикой.
  /// </summary>
  public virtual IList<Ticket> Tickets { get; set; } = new List<Ticket>();
}