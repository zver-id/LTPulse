using System;

namespace WebAPI.DTO;

/// <summary>
/// DTO комманды.
/// </summary>
public class TeamDTO
{
  /// <summary>
  /// ID команды.
  /// </summary>
  public int Id { get; set; }
  
  /// <summary>
  /// Имя команды.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Время последнего успешного расчёта метрик. Null, если расчёт ещё не выполнялся.
  /// </summary>
  public DateTime? LastMetricsCalculated { get; set; }
  
}