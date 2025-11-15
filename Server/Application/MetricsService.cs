using CommonModels.Models;

namespace Application;

/// <summary>
/// Сервис метрик.
/// </summary>
public class MetricsService : GenericService
{
  public Task<List<Metric>> GetMetrics(Team team, int daysCount)
  {
    DateTime beginDate = DateTime.Now - TimeSpan.FromDays(daysCount);
    return Task.Run(() =>
    {
      return this.repository.GetByPredicate<Metric>(x => (
        (x.Date > beginDate) &&
        (x.Team.Equals(team))) );
    });
  }
}