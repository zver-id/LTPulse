using System.Linq.Expressions;
using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

/// <summary>
/// Сервис метрик.
/// </summary>
public class MetricsService : GenericService
{


  /// <summary>
  /// Получить все метрики за количество дней предстоящих текущей дате.
  /// </summary>
  /// <param name="team">Команда.</param>
  /// <param name="daysCount">Количество дней за которые нужно получить данне.</param>
  /// <returns>Список метрик.</returns>
  public Task<List<Dictionary<string, object>>> GetMetrics(Expression<Func<Metric, bool>> filter)
  {
    return Task.Run(() =>
    {
      List<Metric> metrics = this.repository.Get<Metric>(filter);
      var result = new List<Dictionary<string, object>>();
      
      var groupedByDate = metrics.GroupBy(m => m.Date.Date)
        .OrderBy(group => group.Key);
      foreach (var group in groupedByDate)
      {
        var dayData = new Dictionary<string, object>();
        dayData["day"] = group.Key.ToString("dd.MM.yyyy");
        foreach (var metric in group)
        {
          dayData[metric.MetricType.Name] = metric.Value;
        }
        result.Add(dayData);
      }
      return result;
    });
  }
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="repository">Репозиторий.</param>
  public MetricsService(IRepository repository) : base(repository)
  {
  }
  
}