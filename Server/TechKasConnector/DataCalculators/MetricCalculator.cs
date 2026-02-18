using System.Globalization;
using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;
using TechKasConnector.Calendar;
using TechKasConnector.Requisites;

namespace TechKasConnector.DataCalculators;

public class MetricCalculator
{
  #region Поля и свойства
  
  /// <summary>
  /// Справочник обращений.
  /// </summary>
  private TechKasReference tickets;
  
  /// <summary>
  /// Генератор списка обращений в работе.
  /// </summary>
  private TicketListGenerator TicketListGenerator { get; set; }
  
  /// <summary>
  /// Генератор списка обращений.
  /// </summary>
  private GradeListGenerator GradeListGenerator { get; set; }
  
  /// <summary>
  /// Репозиторий.
  /// </summary>
  private IRepository repository;

  /// <summary>
  /// Команда, для которой идет расчет.
  /// </summary>
  private Team team;
  
  /// <summary>
  /// Логгер.
  /// </summary>
  private ILogger logger;
  
  /// <summary>
  /// Логгер.
  /// </summary>
  private CalendarCalculator calendar;
  
  #endregion
  
  # region Методы

  /// <summary>
  /// Рассчитать все метрики.
  /// </summary>
  public void ProcessAllMetrics()
  {
    this.CreateMonthMetrics();
    this.CreateMetric("Старше 2 недель",
      t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(2 * 7) &&
           t.State.State == TicketStatus.InWorkFullString);
    this.CreateMetric("Старше 3 недель",
      t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(3 * 7) &&
           t.State.State == TicketStatus.InWorkFullString);
    this.CreateMetric("Старше 4 недель",
    t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(4 * 7));
    
    this.CreateMetric("Хвост", t => true);
    
    this.CreateMetric("0-8", t => t.TimeInWork <= 8 * 60 && TicketType.IncidentsConsultation.Contains(t.Type));
    this.CreateMetric("8-16", t => t.TimeInWork is > 8 * 60 and < 16 * 60 &&
                                   TicketType.IncidentsConsultation.Contains(t.Type));
    this.CreateMetric("16-24", t => t.TimeInWork > 16 * 60 && t.TimeInWork < 24 * 60 &&
                                    TicketType.IncidentsConsultation.Contains(t.Type));
    this.CreateMetric(">24", t => t.TimeInWork > 24 * 60 && TicketType.IncidentsConsultation.Contains(t.Type));
    
    this.CreateMetric("green", t => t.TimeInWork / t.Priority.TimeToSolve <= 0.25 
                                    && TicketType.IncidentsConsultation.Contains(t.Type));
    this.CreateMetric("sandy", t => t.TimeInWork / t.Priority.TimeToSolve >= 0.25 && 
                                    t.TimeInWork / t.Priority.TimeToSolve < 0.5 
                                    && TicketType.IncidentsConsultation.Contains(t.Type));
    this.CreateMetric("yellow", t => t.TimeInWork / t.Priority.TimeToSolve >= 0.5 &&
                                     t.TimeInWork / t.Priority.TimeToSolve < 0.75
                                     && TicketType.IncidentsConsultation.Contains(t.Type));
    this.CreateMetric("red", t => t.TimeInWork / t.Priority.TimeToSolve >= 0.75
                                  && TicketType.IncidentsConsultation.Contains(t.Type));
    
    this.CreateMetric("Инциденты", t => t.Type == TicketType.Incident && t.IncomingDate == DateTime.Now.Date);
    this.CreateMetric("Консультации", t => t.Type == TicketType.Consultation && t.IncomingDate == DateTime.Now.Date);
    this.CreateMetric("Запросы", t => t.Type == TicketType.Request && t.IncomingDate == DateTime.Now.Date);
    this.CreateMetric("Проблемы", t => t.Type == TicketType.Problem && t.IncomingDate == DateTime.Now.Date);
    this.CreateMetric("Поступило всего", t => t.IncomingDate == DateTime.Now.Date);
    this.CreateGradeMetric("Поступившие", g => g.Date == DateTime.Now.Date );
  }

  /// <summary>
  /// Создать метрики "по месяцам".
  /// </summary>
  private void CreateMonthMetrics()
  {
    var culture = new CultureInfo("ru-RU");
    var monthGroupedTickets = this.TicketListGenerator.Tickets
      .GroupBy(t => culture.TextInfo.ToTitleCase(t.IncomingDate.ToString("MMMM yyyy", culture)))
      .ToDictionary(g => g.Key, g => g.ToList());
    foreach (var monthGroup in monthGroupedTickets)
    {
      var metric = this.GetOrCreateMetric(DateTime.Today, this.GetOrCreateMonthMetricType(monthGroup.Key));
      metric.Value = monthGroup.Value.Count;
      metric.Tickets = monthGroup.Value;
      this.repository.AddOrUpdate(metric);
    }
  }

  /// <summary>
  /// Рассчитать метрику.
  /// </summary>
  /// <param name="nameOfMetricType">Название метрики.</param>
  /// <param name="isInWorkOnly">Признак, что нужно считать только обращения в работе.</param>
  private void CreateMetric(string nameOfMetricType, Predicate<Ticket> predicate)
  {
    var metricType = this.repository.Get<MetricType>(mt => mt.Name == nameOfMetricType).First();
    var metric = this.GetOrCreateMetric(DateTime.Today, metricType);
    var tickets = this.TicketListGenerator.Tickets
        .Where (t => predicate(t))
        .ToList();
    metric.Value = tickets.Count;
    metric.Tickets = tickets;
 
    this.repository.AddOrUpdate(metric);
  }
  
  private void CreateGradeMetric(string nameOfMetricType, Predicate<Grade> predicate)
  {
    var metricType = this.repository.Get<MetricType>(mt => mt.Name == nameOfMetricType).First();
    var metric = this.GetOrCreateMetric(DateTime.Today, metricType);
    var grades = this.GradeListGenerator.Grades
      .Where (t => predicate(t))
      .ToList();
    metric.Value = grades.Count;
    metric.Grades = grades;
 
    this.repository.AddOrUpdate(metric);
  }
  
  /// <summary>
  /// Получить или создать метрику.
  /// </summary>
  /// <param name="date"></param>
  /// <param name="metricType"></param>
  /// <returns></returns>
  private Metric GetOrCreateMetric(DateTime date, MetricType metricType)
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
  /// Получить или создать тип метрики для метрик по месяцам.
  /// </summary>
  /// <param name="metricTypeName">Имя типа метрики.</param>
  /// <returns>Тип метрики.</returns>
  private MetricType GetOrCreateMonthMetricType(string metricTypeName)
  {
    var metricType = this.repository.Get<MetricType>(mt => mt.Name == metricTypeName).FirstOrDefault();
    if (metricType == null)
    {
      var metricGroup = this.repository.Get<MetricGroup>(mt => mt.Name == "Month").First();
      metricType = new MetricType
      {
        Name = metricTypeName,
        MetricGroup = metricGroup
      };
      this.repository.Add(metricType);
    }
    return metricType;
  }
  
  /// <summary>
  /// Инициализация калькулятора.
  /// </summary>
  /// <param name="team">Команда, по которой производится расчет.</param>
  public void Init(Team team)
  {
    this.team = team;
    this.tickets = new TechKasReference("ПДД");
    this.SetInitFilters();
    this.SetEmployeeFilters();
    this.TicketListGenerator = new TicketListGenerator(this.repository, this.tickets, this.team);
    this.GradeListGenerator.GenerateForTeam(this.team);
  }

  /// <summary>
  /// Установить первоначальные фильтры из БД.
  /// </summary>
  private void SetInitFilters()
  {
    List<TechKasFilter> initFilters = this.repository.Get<TechKasFilter>(
        f=> f.Team == null || f.Team.Id == this.team.Id)
      .ToList();
    Dictionary<string, List<string>> includeFilters = initFilters
      .Where(f => f.ShouldInclude)
      .GroupBy(f => f.NameOfField)
      .ToDictionary(
        f => f.Key,
        f => f.Select(f=> f.Value).ToList()
      );
    foreach (var filter in includeFilters)
      this.tickets.SetFilter(filter.Key, filter.Value);
    
    Dictionary<string, List<string>> excludeFilters = initFilters
      .Where(f => !f.ShouldInclude)
      .GroupBy(f => f.NameOfField)
      .ToDictionary(
        f => f.Key,
        f => f.Select(f=> f.Value).ToList()
      );

    foreach (var filter in excludeFilters)
    {
      if (filter.Value.Count == 1)
      {
        this.tickets.SetFilter(filter.Key, filter.Value.First(), "<>");
      }
      else
      {
        foreach (var value in filter.Value)
        {
          this.tickets.SetFilter(filter.Key, value, "<>");
        } 
      }
    }
  }
  
  /// <summary>
  /// Установить фильтры по сотрудникам команды.
  /// </summary>
  private void SetEmployeeFilters()
  {
    var techkasNumbers = this.team.Employees
      .Select(e => e.TechKASNumber).ToList();
    this.tickets.SetFilter(TechKasRequisites.Employee, techkasNumbers);
  }
  
  #endregion
  
  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  public MetricCalculator(IRepository repository, ILogger logger, CalendarCalculator calendar,
    GradeListGenerator gradeListGenerator)
  {
    this.repository = repository;
    this.logger = logger;
    this.calendar = calendar;
    this.GradeListGenerator = gradeListGenerator;
  }
  
  #endregion
}