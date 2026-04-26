using System.Globalization;
using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;
using NHibernate.Infrastructure;
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
  private readonly IRepository repository;

  /// <summary>
  /// Команда, для которой идет расчет.
  /// </summary>
  private Team team;
  
  /// <summary>
  /// Логгер.
  /// </summary>
  private readonly ILogger<MetricCalculator> logger;
  
  /// <summary>
  /// Калькулятор баллов внешних сообщений.
  /// </summary>
  private ExternalMessageCalculator ExternalMessages { get; }
  
  #endregion
  
  # region Методы

  /// <summary>
  /// Рассчитать все метрики.
  /// </summary>
  public async Task ProcessAllMetrics()
  {
    this.logger.LogInformation($"Начало записи метрик для команды {this.team.Name}");
    await this.CreateMonthMetrics();
    await this.CreateMetric("Старше 2 недель",
      t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(2 * 7) &&
           t.State.State == TicketStatus.InWorkFullString &&
      !t.Type.Equals(TicketType.ProblemFull));
    await this.CreateMetric("Старше 3 недель",
      t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(3 * 7) &&
           t.State.State == TicketStatus.InWorkFullString &&
           !t.Type.Equals(TicketType.ProblemFull));
    await this.CreateMetric("Старше 4 недель",
    t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(4 * 7) &&
         !string.Equals(t.State.State, TicketStatus.ClosedFullString) &&
         !t.Type.Equals(TicketType.ProblemFull));
    
    await this.CreateMetric("Хвост", t => !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    
    await this.CreateMetric("0-8", t => t.TimeInWork <= 8 && TicketType.IncidentFull.Equals(t.Type) &&
                                  !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateMetric("8-16", t => t.TimeInWork is > 8 and <= 16 &&
                                   TicketType.IncidentFull.Equals(t.Type) &&
                                   !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateMetric("16-24", t => t.TimeInWork > 16 && t.TimeInWork < 24 &&
                                    TicketType.IncidentFull.Equals(t.Type) &&
                                    !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateMetric(">24", t => t.TimeInWork > 24 && TicketType.IncidentFull.Equals(t.Type) &&
                                  !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    
    await this.CreateMetric("<0.25", t => t.TimeInWork / t.Priority.TimeToSolve <= 0.25 
                                    && TicketType.IncidentFull.Equals(t.Type) &&
                                    !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateMetric("0.25-0.5", t => t.TimeInWork / t.Priority.TimeToSolve >= 0.25 && 
                                    t.TimeInWork / t.Priority.TimeToSolve < 0.5 
                                    && TicketType.IncidentFull.Equals(t.Type) &&
                                    !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateMetric("0.5-0.75", t => t.TimeInWork / t.Priority.TimeToSolve >= 0.5 &&
                                     t.TimeInWork / t.Priority.TimeToSolve < 0.75
                                     && TicketType.IncidentFull.Equals(t.Type) &&
                                     !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateMetric(">0.75", t => t.TimeInWork / t.Priority.TimeToSolve >= 0.75
                                  && TicketType.IncidentFull.Equals(t.Type) &&
                                  !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    
    await this.CreateMetric("Инциденты", t => t.Type == TicketType.IncidentFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateMetric("Консультации", t => t.Type == TicketType.ConsultationFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateMetric("Запросы", t => t.Type == TicketType.RequestFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateMetric("Проблемы", t => t.Type == TicketType.ProblemFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateMetric("Поступило всего", t => t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateMetric("Всего в работе", t => t.State.State.Equals(TicketStatus.InWorkFullString));
    await this.CreateGradeMetric("Поступившие", g => g.Date.Date == DateTime.Now.Date && g.Score == 2 );
    await this.CreateSpentTimeMetric("Затрачено в часах", t => t.Type == TicketType.RequestFull);
    await this.CreateExternalMessageMetric("Внешние сообщения");
    this.logger.LogInformation($"Метрики для команды {this.team.Name} рассчитаны");
  }

  /// <summary>
  /// Создать метрики "по месяцам".
  /// </summary>
  private async Task CreateMonthMetrics()
  {
    var currentMonthMetrics = await this.repository.GetAsync<Metric>(m => m.MetricType.MetricGroup.Name.Equals("Month") &&
                                                               m.Team == this.team &&
                                                               m.Date.Date == DateTime.Now.Date);
    foreach (var metric in currentMonthMetrics)
    {
      await this.repository.Delete(metric);
    }
    var culture = new CultureInfo("ru-RU");
    var monthGroupedTickets = this.TicketListGenerator.Tickets
      .Where(t=> t.State.State.Equals(TicketStatus.InWorkFullString))
      .GroupBy(t => culture.TextInfo.ToTitleCase(t.IncomingDate.ToString("MMMM yyyy", culture)))
      .ToDictionary(g => g.Key, g => g.ToList());
    foreach (var monthGroup in monthGroupedTickets)
    {
      var metric = await this.GetOrCreateMetric(DateTime.Today, await this.GetOrCreateMonthMetricType(monthGroup.Key));
      metric.Value = monthGroup.Value.Count;
      metric.Tickets = monthGroup.Value;
      await this.repository.AddOrUpdate(metric);
    }
  }

  /// <summary>
  /// Рассчитать метрику.
  /// </summary>
  /// <param name="nameOfMetricType">Название метрики.</param>
  /// <param name="predicate">Условие по которому отбираются обращения для метрики.</param>
  private async Task CreateMetric(string nameOfMetricType, Predicate<Ticket> predicate)
  {
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    var metric = await this.GetOrCreateMetric(DateTime.Today, metricType);
    var relatedTickets = this.TicketListGenerator.Tickets
        .Where (t => predicate(t))
        .ToList();
    metric.Value = relatedTickets.Count;
    metric.Tickets = relatedTickets;
 
    await this.repository.AddOrUpdate(metric);
  }

  /// <summary>
  /// Рассчитать метрику.
  /// </summary>
  /// <param name="nameOfMetricType">Название метрики.</param>
  /// <param name="predicate">Условие по которому отбираются обращения для метрики.</param>
  private async Task CreateSpentTimeMetric(string nameOfMetricType, Predicate<Ticket> predicate)
  {
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    var metric = await this.GetOrCreateMetric(DateTime.Today, metricType);
    var relatedTickets = this.TicketListGenerator.Tickets
      .Where (t => predicate(t))
      .ToList();
    metric.Value = relatedTickets
      .Sum(t => t.TimeStampedOnDay);
    metric.Tickets = relatedTickets;
 
    await this.repository.AddOrUpdate(metric);
  }
  
  /// <summary>
  /// Создать метрику по оценкам.
  /// </summary>
  /// <param name="nameOfMetricType">Название метрики.</param>
  /// <param name="predicate">Условие отбора.</param>
  private async Task CreateGradeMetric(string nameOfMetricType, Predicate<Grade> predicate)
  {
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    var metric = await this.GetOrCreateMetric(DateTime.Today, metricType);
    var grades = this.GradeListGenerator.Grades
      .Where (t => predicate(t))
      .ToList();
    metric.Value = grades.Count;
    metric.Grades = grades;
 
    await this.repository.AddOrUpdate(metric);
  }

  /// <summary>
  /// Рассчитать метрику баллов внешних сообщений.
  /// </summary>
  /// <param name="nameOfMetricType">Имя метрики.</param>
  private async Task CreateExternalMessageMetric(string nameOfMetricType)
  {
    try
    {
      var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
      var metric = await this.GetOrCreateMetric(DateTime.Today, metricType);
      var employees = this.team.Employees
        .Select(e => e.Name)
        .ToList();
      
      var totalScore = 0;
      var scores = await this.ExternalMessages.GetEmployeeScores();
      foreach (KeyValuePair<string, int> empScore in scores)
      {
        if (employees.Contains(empScore.Key))
        {
          totalScore += empScore.Value;
        }
      }

      metric.Value = totalScore;
      await this.repository.AddOrUpdate(metric);
    }
    catch (Exception ex)
    {
      this.logger.LogError(ex, "Error creating metric: {nameOfMetricType}", nameOfMetricType);
    }
  }
  
  /// <summary>
  /// Получить или создать метрику.
  /// </summary>
  /// <param name="date"></param>
  /// <param name="metricType"></param>
  /// <returns></returns>
  private async Task<Metric> GetOrCreateMetric(DateTime date, MetricType metricType)
  {
    try
    {
      return await this.repository.GetFirstAsync<Metric>(m =>
        m.Date == date && m.MetricType == metricType && m.Team == this.team);
    }
    catch (InvalidOperationException)
    {
      return new Metric
      {
        Date = date,
        MetricType = metricType,
        Team = this.team
      };
    }
  }

  /// <summary>
  /// Получить или создать тип метрики для метрик по месяцам.
  /// </summary>
  /// <param name="metricTypeName">Имя типа метрики.</param>
  /// <returns>Тип метрики.</returns>
  private async Task<MetricType> GetOrCreateMonthMetricType(string metricTypeName)
  {
    try
    {
      return await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == metricTypeName);
    }
    catch (InvalidOperationException)
    {
      var metricGroup = await this.repository.GetFirstAsync<MetricGroup>(mt => mt.Name == "Month");
      var metricType = new MetricType
      {
        Name = metricTypeName,
        MetricGroup = metricGroup
      };
      this.repository.Add(metricType);
      return metricType;
    }
  }
  
  /// <summary>
  /// Инициализация калькулятора.
  /// </summary>
  /// <param name="team">Команда, по которой производится расчет.</param>
  public async Task Init(Team team)
  {
    this.logger.LogInformation($"Initializing metric calculator for team: {team.Name}");
    this.team = await this.repository.GetById<Team>(team.Id);
    this.tickets = new TechKasReference("ПДД");
    await this.SetInitFilters();
    this.SetEmployeeFilters();
    this.TicketListGenerator = new TicketListGenerator(this.repository, this.tickets, this.team);
    await this.TicketListGenerator.InitTicketList();
    await this.GradeListGenerator.GenerateForTeam(this.team);
    this.logger.LogInformation($"Finish initialize metric calculator for team: {team.Name}");
  }

  /// <summary>
  /// Установить первоначальные фильтры из БД.
  /// </summary>
  private async Task SetInitFilters()
  {
    List<TechKasFilter> initFilters = await this.repository.GetAsync<TechKasFilter>(
        f=> f.Team == null || f.Team.Id == this.team.Id);
    Dictionary<string, List<string>> includeFilters = initFilters
      .Where(f => f.ShouldInclude)
      .GroupBy(f => f.NameOfField)
      .ToDictionary(
        f => f.Key,
        f => f.Select(filter => filter.Value).ToList()
      );
    foreach (var filter in includeFilters)
      this.tickets.SetFilter(filter.Key, filter.Value);
    
    Dictionary<string, List<string>> excludeFilters = initFilters
      .Where(f => !f.ShouldInclude)
      .GroupBy(f => f.NameOfField)
      .ToDictionary(
        f => f.Key,
        f => f.Select(filter => filter.Value).ToList()
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
    if (techkasNumbers.Count == 0)
    {
      this.logger.LogError("В команде нет сотрудников. Расчет прекращен.");
      throw new ArgumentException("В команде нет сотрудников. Расчет прекращен.");
    }
    this.tickets.SetFilter(TechKasRequisites.Employee, techkasNumbers);
  }
  
  #endregion
  
  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  public MetricCalculator(IRepository repository, ILogger<MetricCalculator> logger, 
    GradeListGenerator gradeListGenerator, ExternalMessageCalculator externalMessages)
  {
    this.repository = repository;
    this.logger = logger;
    this.GradeListGenerator = gradeListGenerator;
    this.ExternalMessages = externalMessages;
  }
  #endregion
}