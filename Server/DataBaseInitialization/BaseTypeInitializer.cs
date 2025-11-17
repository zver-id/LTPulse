using System.Globalization;
using CommonModels.Interfaces;
using CommonModels.Models;
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
    Dictionary<string, MetricGroup> metricGroups = new Dictionary<string, MetricGroup>
    {
      ["Month"] = new MetricGroup { Name = "Month" },
      ["Older3Week"] = new MetricGroup { Name = "Older3Week" },
      ["Snowball"] = new MetricGroup { Name = "Snowball" },
      ["Tail"] = new MetricGroup { Name = "Tail" },
      ["ColorZones"] = new MetricGroup { Name = "ColorZones" },
      ["ColorZonesPriority"] = new MetricGroup { Name = "ColorZonesPriority" },
      ["NegativeGrades"] = new MetricGroup { Name = "NegativeGrades" },
      ["ExternalMessages"] = new MetricGroup { Name = "ExternalMessages" },
      ["IncomingTypes"] = new MetricGroup { Name = "IncomingTypes" },
      ["IncomingTotal"] = new MetricGroup { Name = "IncomingTotal" },
      ["SpentForRequests"] = new MetricGroup { Name = "SpentForRequests" }
    };
    
    List<IHasId> metricTypes =
    [
      new MetricType{ Name = "Старше 2 недель", MetricGroup = metricGroups["Older3Week"] },
      new MetricType{Name = "Старше 3 недель",  MetricGroup = metricGroups["Older3Week"] }, 
      new MetricType{Name = "Старше 4 недель",   MetricGroup = metricGroups["Snowball"] },
      new MetricType{Name = "Всего в работе", MetricGroup = metricGroups["Month"] },
      new MetricType{Name = "Хвост", MetricGroup = metricGroups["Tail"] },
      new MetricType{Name = "Внешние сообщения",  MetricGroup = metricGroups["ExternalMessages"] },
      new MetricType{Name = "0-8", MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = "8-16",  MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = "16-24",  MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = ">24",  MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = "Поступившие", MetricGroup = metricGroups["NegativeGrades"] },
      new MetricType{Name = "Проработанные",  MetricGroup = metricGroups["NegativeGrades"] },
      new MetricType{Name = "Инциденты",   MetricGroup = metricGroups["IncomingTypes"] },
      new MetricType{Name = "Консультации", MetricGroup = metricGroups["IncomingTypes"] },
      new MetricType{Name = "Запросы", MetricGroup = metricGroups["IncomingTypes"] },
      new MetricType{Name = "Проблемы", MetricGroup = metricGroups["IncomingTypes"] },
      new MetricType{Name = "Поступило всего", MetricGroup = metricGroups["IncomingTypes"] },
      new MetricType{Name = "Затрачено в часах", MetricGroup = metricGroups["SpentForRequests"] }
    ];

    List<IHasId> groups = metricGroups.Values.Cast<IHasId>().ToList();
    this.TryAddTypes(groups);
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
          var metricGroup = this.dbRepository.GetByPredicate<MetricGroup>(x => x.Name == "Month").FirstOrDefault();
          metricType = new MetricType { Name = metric.Key,  MetricGroup = metricGroup! };
          this.dbRepository.Add(metricType);
        }

        DateTime date;
        try
        {
          date = DateTime.ParseExact(dayMetric.Key, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
          date = DateTime.ParseExact(dayMetric.Key, "d.MM.yyyy", CultureInfo.InvariantCulture);
        }
        var metricToSave = new Metric
        {
          Date = date,
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