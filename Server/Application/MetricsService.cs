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
  public Task<List<Dictionary<string, object>>> GetMetrics(Team team, int daysCount)
  {
    DateTime beginDate = DateTime.Now - TimeSpan.FromDays(daysCount);
    return Task.Run(() =>
    {
      List<Metric> metrics = this.repository.GetByPredicate<Metric>(x => (
        (x.Date > beginDate) &&
        (x.Team.Equals(team))) );
      var result = new List<Dictionary<string, object>>();

      
      var groupedByDate = metrics.GroupBy(m => m.Date.Date);
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
}