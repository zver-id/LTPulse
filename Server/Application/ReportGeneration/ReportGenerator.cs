using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application.ReportGeneration;

/// <summary>
/// Генератор отчётов.
/// </summary>
public class ReportGenerator
{
  /// <summary>
  /// Репозиторий.
  /// </summary>
  public IRepository Repository { get; }
  
  /// <summary>
  /// Команда.
  /// </summary>
  public Team Team { get; }

  /// <summary>
  /// Получить все метрики команды.
  /// </summary>
  /// <returns></returns>
  public List<Metric> GetAllMetrics()
  {
    return this.Repository.Get<Metric>(m => m.Team == this.Team);
  }
}