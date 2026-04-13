using System;
using CommonModels.Interfaces;

namespace WebAPI.DTO;

/// <summary>
/// DTO метрики.
/// </summary>
public class MetricDTO
{
  /// <summary>
  /// ID метрики.
  /// </summary>
  public int Id { get; set; }
  
  /// <summary>
  /// Дата метрики.
  /// </summary>
  public DateTime Date { get; set; }
  
  /// <summary>
  /// Команда, которой принадлежит метрика.
  /// </summary>
  public string Team { get; set; }
  
  /// <summary>
  /// Тип метрики.
  /// </summary>
  public string MetricType { get; set; }
  
  /// <summary>
  /// Значение метрики.
  /// </summary>
  public float Value { get; set; }
}