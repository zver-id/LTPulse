using CommonModels.Models;
using DBCore;
using TechKasConnector.DataCalculators;

namespace TechKasConnector.MetricCreators;

public class GenericMetricCreator
{
  private readonly MetricCalculator metricCalculator;
  private readonly DBRepository repository;
  private readonly Team team;
  public Metric GetOrCreateMetric(DateTime date, MetricType metricType)
  {
    var metric = this.repository.Get<Metric>(m =>
      m.Date == date && m.MetricType == metricType && m.Team == this.team)
      .FirstOrDefault();
    if (metric == null)
    {
      return new Metric
      {
        Date = date,
        MetricType = metricType,
        Team = this.team
      };
    }
    else
    {
      return metric;
    }
  }

  /// <summary>
  /// Сохранить метрики по месяцам.
  /// </summary>
  public void CreateMonthMetrics()
  {
    Dictionary<string, int> monthsCount = this.metricCalculator.GetCountTicketInProgressByMonth();
    foreach (KeyValuePair<string, int> monthCount in monthsCount)
    {
      var metricType = this.repository.Get<MetricType>(mt => mt.Name == monthCount.Key).FirstOrDefault();
      if (metricType == null)
      {
        var metricGroup = this.repository.Get<MetricGroup>(mt => mt.Name == "Month").First();
        metricType = new MetricType
        {
          Name = monthCount.Key,
          MetricGroup = metricGroup
        };
        this.repository.Add(metricType);
      }

      var newMetric = this.GetOrCreateMetric(DateTime.Today, metricType);
      newMetric.Value = monthCount.Value;
      this.repository.AddOrUpdate(newMetric);
    }
  }

  public GenericMetricCreator(DBRepository? repository,  Team team)
  {
    this.metricCalculator = new MetricCalculator(team);
    this.repository = repository;
    this.team = team;
  }
}