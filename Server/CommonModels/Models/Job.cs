using System;
using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Фоновый процесс.
/// </summary>
public class Job : IHasId
{
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Наименование процесса.
  /// </summary>
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Время следующего запуска.
  /// </summary>
  public virtual DateTime? StartProcess { get; set; }
  
  /// <summary>
  /// Интервал воспроизведения.
  /// </summary>
  public virtual TimeSpan RepeatInterval { get; set; }
  
  /// <summary>
  /// Команда, для которой рассчитывается процесс.
  /// </summary>
  public virtual Team Team { get; set; }
  
  /// <summary>
  /// Признак, что процесс запущен.
  /// </summary>
  public virtual bool InProgress { get; set; }
}