using Common.Interfaces;
using Common.Models;
using DBCore;

namespace DataBaseInitialization;

/// <summary>
/// Первоначальное заполнение базы данных.
/// </summary>
public class BaseTypeInitializer
{
  /// <summary>
  /// Репозиторий.
  /// </summary>
  private DBRepository dbRepository { get; init; }
  
  /// <summary>
  /// Записать типы метрик.
  /// </summary>
  public void AddMetricTypes()
  {
    List<IHasId> metricTypes =
    [
      new MetricType("OlderThanTwoWeeks"),
      new MetricType("OlderThanThreeWeeks"),
      new MetricType("OlderFourTwoWeeks"),
      new MetricType("TotalInProgress"),
      new MetricType("NotClosed"),
      new MetricType("ExternalMessages"),
      new MetricType("GreenZone"),
      new MetricType("SandyZone"),
      new MetricType("YellowZone"),
      new MetricType("RedZone"),
      new MetricType("NegativeGrades"),
      new MetricType("WorkedNegativeGrades"),
      new MetricType("IncomingIncidents"),
      new MetricType("IncomingConsultations"),
      new MetricType("IncomingRequests"),
      new MetricType("IncomingProblems"),
      new MetricType("IncomingTotal"),
      new MetricType("SpentOnRequests")
    ];

    this.TryAddTypes(metricTypes);
  }

  /// <summary>
  /// Записать команды.
  /// </summary>
  public void AddTeams()
  {
    List<IHasId> teams =
    [
      new Team {Name = "Аврора"},
      new Team {Name = "Атлас"},
      new Team {Name = "Бета"},
      new Team {Name = "ОГВ"},
      new Team {Name = "Sokongan"},
      new Team {Name = "Sierra Dogs"},
    ];
    
    this.TryAddTypes(teams);
  }

  /// <summary>
  /// Записать метрики из существующего файла excel.
  /// </summary>
  /// <param name="fileName">Имя файла с данными.</param>
  /// <param name="sheetName">Имя листа с данными.</param>
  /// <param name="teamName">Имя команды.</param>
  /// <exception cref="ArgumentException">Переданная команда не существует.</exception>
  public void AddTeamMetricsFromExcel(string fileName, string sheetName, string teamName)
  {
    var team = this.dbRepository.GetByPredicate<Team>(x=>x.Name == teamName).FirstOrDefault();
    if (team == null)
      throw new ArgumentException("Team not found");
    var metricDict = ExcelParser.ParseExcelToDictionaries(fileName,  sheetName);
    foreach (var dayMetric in metricDict)
    {
      foreach (var metric in dayMetric.Value)
      {
        if (metric.Value == 0)
          continue;
        var metricType = this.dbRepository.GetByPredicate<MetricType>(x => x.Name == metric.Key).FirstOrDefault();
        if (metricType == null)
        {
          metricType = new MetricType { Name = metric.Key };
          this.dbRepository.Add(metricType);
        }
        var metricToSave = new Metric
        {
          Date = dayMetric.Key,
          Team = team,
          MetricType = metricType,
          Value = metric.Value
        };
        this.dbRepository.Add(metricToSave);
      }
    }
  }
  
  /// <summary>
  /// Попробовать добавить новый объект.
  /// </summary>
  /// <param name="types">Новый объект.</param>
  private void TryAddTypes(List<IHasId> types)
  {
    foreach (var item in types)
    {
      try
      {
        this.dbRepository.Add(item);
        Console.WriteLine($"Объект {nameof(item)} с {item.Id} добавлен");
      }
      catch (NHibernate.Exceptions.GenericADOException ex)
      {
        Console.WriteLine($"Объект {nameof(item)} с {item.Id} пропущен");
      }
    }
  }

  public BaseTypeInitializer(DBRepository repository)
  {
    this.dbRepository = repository;
  }
}