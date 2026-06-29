using System.Linq.Expressions;
using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

/// <summary>
///   Сервис метрик.
/// </summary>
public class MetricsService(IRepository repository) : GenericService(repository)
{
  /// <summary>
  ///   Получить все метрики за количество дней предстоящих текущей дате.
  /// </summary>
  /// <returns>Список метрик.</returns>
  public async Task<List<Dictionary<string, object>>> GetMetrics(Expression<Func<Metric, bool>> filter)
  {
    var metrics = await this.repository.GetAsync(filter);
    var result = new List<Dictionary<string, object>>();

    var groupedByDate = metrics.GroupBy(m => m.Date.Date)
      .OrderBy(group => group.Key);
    foreach (var group in groupedByDate)
    {
      var dayData = new Dictionary<string, object>();
      dayData["day"] = group.Key.ToString("dd.MM.yyyy");
      foreach (var metric in group) dayData[metric.MetricType.Name] = metric.Value;
      result.Add(dayData);
    }
    return result;
  }
}