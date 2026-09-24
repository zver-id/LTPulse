using System.Globalization;
using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;
using NHibernate.Infrastructure;
using TechKasConnector.Calendar;
using TechKasConnector.Mattermost;
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
  
  /// <summary>
  /// Календарь рабочего времени.
  /// </summary>
  private CalendarCalculator Calendar {get; init;}
  
  /// <summary>
  /// Клиент Mattermost.
  /// </summary>
  private MattermostClient Mattermost { get; }
  
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
  /// Рассчитать метрики для каждого сотрудника команды.
  /// </summary>
  public async Task ProcessEmployeeMetrics()
  {
    this.logger.LogInformation($"Начало расчета метрик для сотрудников команды {this.team.Name}");
    foreach (var employee in this.team.Employees)
    {
      await this.ProcessEmployeeMetricsFor(employee);
    }
    this.logger.LogInformation($"Метрики для сотрудников команды {this.team.Name} рассчитаны");
  }

  /// <summary>
  /// Рассчитать метрики для одного сотрудника.
  /// </summary>
  /// <param name="employee">Сотрудник.</param>
  private async Task ProcessEmployeeMetricsFor(Employee employee)
  {
    this.logger.LogInformation($"Расчет метрик для сотрудника {employee.Name}");
    var tickets = this.TicketListGenerator.Tickets
      .Where(t => t.Employee == employee.Name)
      .ToList();
    var grades = this.GradeListGenerator.Grades
      .Where(g => g.Ticket?.Employee == employee.Name)
      .ToList();

    await this.CreateEmployeeMonthMetrics(employee, tickets);
    await this.CreateEmployeeMetric(employee, tickets, "Старше 2 недель",
      t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(2 * 7) &&
           t.State.State == TicketStatus.InWorkFullString &&
           !t.Type.Equals(TicketType.ProblemFull));
    await this.CreateEmployeeMetric(employee, tickets, "Старше 3 недель",
      t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(3 * 7) &&
           t.State.State == TicketStatus.InWorkFullString &&
           !t.Type.Equals(TicketType.ProblemFull));
    await this.CreateEmployeeMetric(employee, tickets, "Старше 4 недель",
      t => DateTime.Now - t.IncomingDate > TimeSpan.FromDays(4 * 7) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString) &&
           !t.Type.Equals(TicketType.ProblemFull));
    await this.CreateEmployeeMetric(employee, tickets, "Хвост",
      t => !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, "0-8",
      t => t.TimeInWork <= 8 && TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, "8-16",
      t => t.TimeInWork is > 8 and <= 16 && TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, "16-24",
      t => t.TimeInWork > 16 && t.TimeInWork < 24 && TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, ">24",
      t => t.TimeInWork > 24 && TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, "<0.25",
      t => t.TimeInWork / t.Priority.TimeToSolve <= 0.25 && TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, "0.25-0.5",
      t => t.TimeInWork / t.Priority.TimeToSolve >= 0.25 && t.TimeInWork / t.Priority.TimeToSolve < 0.5 &&
           TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, "0.5-0.75",
      t => t.TimeInWork / t.Priority.TimeToSolve >= 0.5 && t.TimeInWork / t.Priority.TimeToSolve < 0.75 &&
           TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, ">0.75",
      t => t.TimeInWork / t.Priority.TimeToSolve >= 0.75 && TicketType.IncidentFull.Equals(t.Type) &&
           !string.Equals(t.State.State, TicketStatus.ClosedFullString));
    await this.CreateEmployeeMetric(employee, tickets, "Инциденты",
      t => t.Type == TicketType.IncidentFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateEmployeeMetric(employee, tickets, "Консультации",
      t => t.Type == TicketType.ConsultationFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateEmployeeMetric(employee, tickets, "Запросы",
      t => t.Type == TicketType.RequestFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateEmployeeMetric(employee, tickets, "Проблемы",
      t => t.Type == TicketType.ProblemFull && t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateEmployeeMetric(employee, tickets, "Поступило всего",
      t => t.IncomingDate.Date == DateTime.Now.Date);
    await this.CreateEmployeeMetric(employee, tickets, "Всего в работе",
      t => t.State.State.Equals(TicketStatus.InWorkFullString));
    await this.CreateEmployeeGradeMetric(employee, grades, "Поступившие",
      g => g.Date.Date == DateTime.Now.Date && g.Score == 2);
    await this.CreateEmployeeSpentTimeMetric(employee, tickets, "Затрачено в часах",
      t => t.Type == TicketType.RequestFull);
    await this.CreateEmployeeExternalMessageMetric(employee);
    this.logger.LogInformation($"Метрики для сотрудника {employee.Name} рассчитаны");
  }

  private async Task CreateEmployeeMonthMetrics(Employee employee, List<Ticket> tickets)
  {
    var currentMonthMetrics = await this.repository.GetAsync<Metric>(m =>
      m.MetricType.MetricGroup.Name.Equals("Month") &&
      m.Team == this.team &&
      m.Employee == employee &&
      m.Date.Date == DateTime.Now.Date);
    foreach (var metric in currentMonthMetrics)
      await this.repository.Delete(metric);

    var culture = new CultureInfo("ru-RU");
    var monthGroupedTickets = tickets
      .Where(t => t.State.State.Equals(TicketStatus.InWorkFullString))
      .GroupBy(t => culture.TextInfo.ToTitleCase(t.IncomingDate.ToString("MMMM yyyy", culture)))
      .ToDictionary(g => g.Key, g => g.ToList());
    foreach (var monthGroup in monthGroupedTickets)
    {
      var metricType = await this.GetOrCreateMonthMetricType(monthGroup.Key);
      var metric = await this.GetOrCreateEmployeeMetric(DateTime.Today, metricType, employee);
      metric.Value = monthGroup.Value.Count;
      metric.Tickets = monthGroup.Value;
      await this.repository.AddOrUpdate(metric);
    }
  }

  private async Task CreateEmployeeMetric(Employee employee, List<Ticket> tickets,
    string nameOfMetricType, Predicate<Ticket> predicate)
  {
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    var metric = await this.GetOrCreateEmployeeMetric(DateTime.Today, metricType, employee);
    var relatedTickets = tickets.Where(t => predicate(t)).ToList();
    metric.Value = relatedTickets.Count;
    metric.Tickets = relatedTickets;
    await this.repository.AddOrUpdate(metric);
  }

  private async Task CreateEmployeeGradeMetric(Employee employee, List<Grade> grades,
    string nameOfMetricType, Predicate<Grade> predicate)
  {
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    var metric = await this.GetOrCreateEmployeeMetric(DateTime.Today, metricType, employee);
    var relatedGrades = grades.Where(g => predicate(g)).ToList();
    metric.Value = relatedGrades.Count;
    metric.Grades = relatedGrades;
    await this.repository.AddOrUpdate(metric);
  }

  private async Task CreateEmployeeSpentTimeMetric(Employee employee, List<Ticket> tickets,
    string nameOfMetricType, Predicate<Ticket> predicate)
  {
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    var metric = await this.GetOrCreateEmployeeMetric(DateTime.Today, metricType, employee);
    var relatedTickets = tickets.Where(t => predicate(t)).ToList();
    metric.Value = relatedTickets.Sum(t => t.TimeStampedOnDay);
    metric.Tickets = relatedTickets;
    await this.repository.AddOrUpdate(metric);
  }

  private async Task CreateEmployeeExternalMessageMetric(Employee employee)
  {
    try
    {
      var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == "Внешние сообщения");
      var metric = await this.GetOrCreateEmployeeMetric(DateTime.Today, metricType, employee);
      var scores = await this.ExternalMessages.GetEmployeeScores();
      var totalScore = scores.TryGetValue(employee.Name, out var score) ? score : 0;
      metric.Value = totalScore;
      await this.repository.AddOrUpdate(metric);
    }
    catch (Exception ex)
    {
      this.logger.LogError(ex, "Error creating employee external message metric for {employee}", employee.Name);
    }
  }

  private async Task<Metric> GetOrCreateEmployeeMetric(DateTime date, MetricType metricType, Employee employee)
  {
    try
    {
      return await this.repository.GetFirstAsync<Metric>(m =>
        m.Date == date && m.MetricType == metricType && m.Team == this.team && m.Employee == employee);
    }
    catch (InvalidOperationException)
    {
      return new Metric
      {
        Date = date,
        MetricType = metricType,
        Team = this.team,
        Employee = employee
      };
    }
  }

  /// <summary>
  /// Создать метрики "по месяцам".
  /// </summary>
  private async Task CreateMonthMetrics()
  {
    this.logger.LogInformation($"Рассчет метрик по месяцам для команды {this.team.Name}");
    var currentMonthMetrics = await this.repository.GetAsync<Metric>(m => m.MetricType.MetricGroup.Name.Equals("Month") &&
                                                               m.Team == this.team &&
                                                               m.Date.Date == DateTime.Now.Date);
    foreach (var metric in currentMonthMetrics)
    {
      await this.repository.Delete(metric);
    }
    var culture = new CultureInfo("ru-RU");
    Dictionary<string, List<Ticket>> monthGroupedTickets = this.TicketListGenerator.Tickets
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
    this.logger.LogInformation($"Метрики по месяцам для команды {this.team.Name} рассчитаны");
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
  /// Рассчитать метрику по затраченному времени.
  /// </summary>
  /// <param name="nameOfMetricType">Название метрики.</param>
  /// <param name="predicate">Условие по которому отбираются обращения для метрики.</param>
  private async Task CreateSpentTimeMetric(string nameOfMetricType, Predicate<Ticket> predicate)
  {
    this.logger.LogInformation($"Расчет метрики отмеченного времени для команды {this.team.Name}");
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    Metric metric = await this.GetOrCreateMetric(DateTime.Today, metricType);
    List<Ticket> relatedTickets = this.TicketListGenerator.Tickets
      .Where (t => predicate(t))
      .ToList();
    metric.Value = relatedTickets
      .Sum(t => t.TimeStampedOnDay);
    metric.Tickets = relatedTickets;
 
    await this.repository.AddOrUpdate(metric);
    this.logger.LogInformation($"Метрика отмеченного времени для команды {this.team.Name} рассчитана");
  }
  
  /// <summary>
  /// Создать метрику по оценкам.
  /// </summary>
  /// <param name="nameOfMetricType">Название метрики.</param>
  /// <param name="predicate">Условие отбора.</param>
  private async Task CreateGradeMetric(string nameOfMetricType, Predicate<Grade> predicate)
  {
    var metricType = await this.repository.GetFirstAsync<MetricType>(mt => mt.Name == nameOfMetricType);
    Metric metric = await this.GetOrCreateMetric(DateTime.Today, metricType);
    List<Grade> grades = this.GradeListGenerator.Grades
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
      Metric metric = await this.GetOrCreateMetric(DateTime.Today, metricType);
      List<string> employees = this.team.Employees
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
  /// <param name="targetTeam">Команда, по которой производится расчет.</param>
  public async Task Init(int teamId)
  {
    this.team = await this.repository.GetById<Team>(teamId);
    this.logger.LogInformation($"Initializing metric calculator for team: {this.team.Name}");
    using (var tickets = new TechKasReference("ПДД"))
    {
      this.tickets = tickets;
      await this.SetInitFilters();
      this.logger.LogInformation($"Установлены начальные фильтры для команды {this.team.Name}");
      this.SetEmployeeFilters();
      this.logger.LogInformation($"Установлены фильтры по сотрудникам для команды {this.team.Name}");
      this.TicketListGenerator = new TicketListGenerator(this.repository, this.tickets, this.Calendar, this.team);
      this.logger.LogDebug($"Call TicketListGenerator.InitTicketList {this.team.Name} ");
      await this.TicketListGenerator.InitTicketList();
      this.logger.LogDebug($"Finish TicketListGenerator.InitTicketLis {this.team.Name}");
      this.logger.LogDebug($"Call GradeListGenerator.GenerateForTeam {this.team.Name}");
      await this.GradeListGenerator.GenerateForTeam(this.team);
      this.logger.LogDebug($"Finish GradeListGenerator.GenerateForTeam {this.team.Name}");
    }
    await this.PopulateEscalations();
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

    foreach (KeyValuePair<string, List<string>> filter in excludeFilters)
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
  
  /// <summary>
  /// Заполнить эскалации для всех обращений через Mattermost.
  /// </summary>
  private async Task PopulateEscalations()
  {
    var tickets = this.TicketListGenerator.Tickets;
    if (tickets.Count == 0) return;

    var channels = await this.repository.GetAsync<CommonModels.Models.MattermostChannel>(_ => true);
    var lineChannelIds = channels.Where(c => c.Type == ChannelType.Line).Select(c => c.ChannelId).ToList();
    var devChannelIds = channels.Where(c => c.Type == ChannelType.Dev).Select(c => c.ChannelId).ToList();

    this.logger.LogInformation($"Заполнение эскалаций для {tickets.Count} обращений");

    foreach (var ticket in tickets)
    {
      var pattern = ticket.Id.ToString();
      try
      {
        ticket.LineEscalationsData = string.Join('|', await this.Mattermost
          .SearchChannelsByRegex(lineChannelIds, pattern));
        ticket.DevsEscalationsData = string.Join('|', await this.Mattermost
          .SearchChannelsByRegex(devChannelIds, pattern));
        await this.repository.AddOrUpdate(ticket);
      }
      catch (Exception ex)
      {
        this.logger.LogError(ex, "Ошибка при заполнении эскалаций для обращения {id}", ticket.Id);
      }
    }
  }
  
  #endregion
  
  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  public MetricCalculator(IRepository repository, ILogger<MetricCalculator> logger, 
    GradeListGenerator gradeListGenerator, ExternalMessageCalculator externalMessages, 
    CalendarCalculator calendar, MattermostClient mattermost)
  {
    this.repository = repository;
    this.logger = logger;
    this.GradeListGenerator = gradeListGenerator;
    this.ExternalMessages = externalMessages;
    this.Calendar = calendar;
    this.Mattermost = mattermost;
  }
  #endregion
}