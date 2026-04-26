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
  private DbRepository dbRepository { get; init; }
  
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
      new MetricType{Name = "Старше 2 недель", MetricGroup = metricGroups["Older3Week"] },
      new MetricType{Name = "Старше 3 недель",  MetricGroup = metricGroups["Older3Week"] }, 
      new MetricType{Name = "Старше 4 недель",   MetricGroup = metricGroups["Snowball"] },
      new MetricType{Name = "Всего в работе", MetricGroup = metricGroups["Month"] },
      new MetricType{Name = "Хвост", MetricGroup = metricGroups["Tail"] },
      new MetricType{Name = "Внешние сообщения",  MetricGroup = metricGroups["ExternalMessages"] },
      new MetricType{Name = "0-8", MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = "8-16",  MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = "16-24",  MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = ">24",  MetricGroup = metricGroups["ColorZones"] },
      new MetricType{Name = "<0.25", MetricGroup = metricGroups["ColorZonesPriority"] },
      new MetricType{Name = "0.25-0.5",  MetricGroup = metricGroups["ColorZonesPriority"] },
      new MetricType{Name = "0.5-0.75",  MetricGroup = metricGroups["ColorZonesPriority"] },
      new MetricType{Name = ">0.75",  MetricGroup = metricGroups["ColorZonesPriority"] },
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
  /// Инициализировать приоритеты.
  /// </summary>
  public void AddPriorities()
  {
    List<IHasId> priorities =
    [
      new Priority { Name = "Критический", TimeToSolve = 4 * 60 },
      new Priority { Name = "Высокий", TimeToSolve = 8 * 60 },
      new Priority { Name = "Средний", TimeToSolve = 16 * 60 },
      new Priority { Name = "Низкий", TimeToSolve = 80 * 60 },
      new Priority { Name = "Планируемый", TimeToSolve = 168 * 60 }
    ];
    
    this.TryAddTypes(priorities);
  }

  /// <summary>
  /// Инициализировать статусы.
  /// </summary>
  public void AddTicketStates()
  {
    List<IHasId> ticketStates =
    [
      new TicketState { State = "Инициализация" },
      new TicketState { State = "В работе" },
      new TicketState { State = "На контроле" },
      new TicketState { State = "Переадресовано" },
      new TicketState { State = "На закрытии" },
      new TicketState { State = "Закрыто" }
    ];
    this.TryAddTypes(ticketStates);
  }

  /// <summary>
  /// Записать метрики из существующего файла excel.
  /// </summary>
  /// <param name="fileName">Имя файла с данными.</param>
  /// <param name="sheetName">Имя листа с данными.</param>
  /// <param name="teamName">Имя команды.</param>
  /// <exception cref="InvalidOperationException">Переданная команда не существует.</exception>
  public async Task AddTeamMetricsFromExcel(string fileName, string sheetName, string teamName)
  {
    var team = await this.dbRepository.GetFirstAsync<Team>(x=>x.Name == teamName);

    var metricDict = ExcelParser.ParseExcelToDictionaries(fileName,  sheetName);
    foreach (var dayMetric in metricDict)
    {
      foreach (var metric in dayMetric.Value)
      {
        if (metric.Value == 0)
          continue;
        MetricType metricType;
        try
        {
          metricType = await this.dbRepository.GetFirstAsync<MetricType>(x => x.Name == metric.Key);
        }
        catch (InvalidOperationException)
        {
          var metricGroup = await this.dbRepository.GetFirstAsync<MetricGroup>(x => x.Name == "Month");
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

  public BaseTypeInitializer(DbRepository repository)
  {
    this.dbRepository = repository;
  }
}