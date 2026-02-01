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
  private Team Team { get; set; }

  /// <summary>
  /// Получить все метрики команды.
  /// </summary>
  /// <returns></returns>
  public List<Metric> GetAllMetricsForTeam(Team team)
  {
    this.Team = team;
    return this.Repository.Get<Metric>(m => m.Team == this.Team);
  }

  public ReportGenerator(IRepository repository)
  {
    this.Repository = repository;
  }
}