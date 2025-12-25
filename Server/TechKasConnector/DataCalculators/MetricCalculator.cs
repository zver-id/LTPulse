using System.Globalization;
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
  /// Репозиторий.
  /// </summary>
  private DBRepository repository;

  /// <summary>
  /// Команда, для которой идет расчет.
  /// </summary>
  private Team team;
  
  #endregion
  
  #region Методы

  /// <summary>
  /// Получить количество обращений по типу.
  /// </summary>
  /// <param name="type">Тип обращения.</param>
  /// <returns>Количество обращений.</returns>
  public int GetTicketCountByType(string type)
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    filter.AddFilter(TechKasRequisites.TicketType, type);
    return this.tickets.Count;
  }

  /// <summary>
  /// Получить количество обращений по типу, поступивших за день.
  /// </summary>
  /// <param name="type">Тип обращений.</param>
  /// <param name="daysAgo">Количество дней назад за которое надо получить сведения.</param>
  /// <returns>Количество обращений.</returns>
  public int GetIncomingTicketsCountByType(string type, int daysAgo)
  {
    var dateForCalculation = new List<string>();
    var currentDay = DateTime.Today - new TimeSpan(daysAgo, 0, 0, 0);
    if (currentDay.DayOfWeek == DayOfWeek.Monday)
    {
      dateForCalculation.Add(currentDay.ToString("dd.MM.yyyy"));
      dateForCalculation.Add((currentDay - new TimeSpan(1, 0,0,0)).ToString("dd.MM.yyyy"));
      dateForCalculation.Add((currentDay - new TimeSpan(2, 0,0,0)).ToString("dd.MM.yyyy"));
    }
    else
    {
      dateForCalculation.Add(currentDay.ToString("dd.MM.yyyy"));
    }

    using var filter = new ReferenceFilterManager(this.tickets);
    filter.AddFilter(TechKasRequisites.OpenDate, dateForCalculation);
    return this.GetTicketCountByType(type);
  }

  /// <summary>
  /// Количество обращений в работе по месяцам создания.
  /// </summary>
  /// <returns>Количество обращений в работе по месяцам создания.</returns>
  public Dictionary<string, int> GetCountTicketInProgressByMonth()
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    var result = new Dictionary<string, int>();
    filter.AddFilter(TechKasRequisites.TicketStatus, "Р");
    
    foreach (TechKasElement ticket in this.tickets)
    {
      DateTime dateOfCreate = DateTime.ParseExact(
        ticket.GetRequisite(TechKasRequisites.OpenDate, RequisitesMode.AsString),
        "dd.MM.yyyy", CultureInfo.InvariantCulture);
      string monthName = dateOfCreate.ToString("MMMM", new CultureInfo("ru-RU"));
      string month = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(monthName)} {dateOfCreate.Year}";
      if (!result.ContainsKey(month))
        result.Add(month, 1);
      else
        result[month]++;
    }
    return result;
  }
  
  /// <summary>
  /// Получить количество обращений старше определенного количества дней.
  /// </summary>
  /// <param name="daysAgo">Количество дней.</param>
  /// <param name="activeOnly">Флаг учитывать ли обращения на контроле.</param>
  /// <returns>Количесвто обращений.</returns>
  public int GetSnowballTicketsCount(int daysAgo = -1, bool activeOnly = false)
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    if (activeOnly)
      filter.AddFilter(TechKasRequisites.TicketStatus, TicketStatus.InWork);
    else
      filter.AddFilter(TechKasRequisites.TicketStatus, TicketStatus.Active);

    filter.AddFilter(TechKasRequisites.TicketType, TicketType.WithoutProblems);
    // Убираем анонимки (Код-338)
    filter.AddFilter(TechKasRequisites.SupportArea, "30262732");
    if (daysAgo != -1)
      filter.AddFilter(TechKasRequisites.OpenDate,
        DateTime.Now.AddDays(-daysAgo).ToString("dd.MM.yyyy"), "<=");
    return this.tickets.Count;
  }

  /// <summary>
  /// Считает количество времени, затраченное на запросы.
  /// </summary>
  /// <param name="daysAgo">Количество дней назад, за которое нужно считать.</param>
  /// <returns>Количество затраченного времени.</returns>
  public float GetTimeSpentOnRequests(int daysAgo = 0)
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    filter.AddFilter(TechKasRequisites.TicketType, TicketType.Request);
    filter.AddFilter(TechKasRequisites.TicketStatus, TicketStatus.Closed, "<>");
    float total = 0;
    var employeeNames = this.team.Employees.Select(e => e.Name);
    foreach (TechKasElement ticket in this.tickets)
    {
      Autoclicker.ClickYes();
      var detail = ticket.GetDetail(2);
      foreach (TechKasElement record in detail)
      {
        bool isActualDate = record.GetRequisite(TechKasRequisites.DateDetail, RequisitesMode.AsString)
                            == DateTime.Now.AddDays(-daysAgo).ToString("dd.MM.yyyy");
        bool employeeInTeam =
          employeeNames.Contains(record.GetRequisite(TechKasRequisites.EmployeeDetail, RequisitesMode.DisplayText));
        if (isActualDate && employeeInTeam)
        {
          total += float.Parse(record.GetRequisite(TechKasRequisites.TimeSpent, RequisitesMode.AsString));
        }
      }
      return total;
    }
    return 0;
  }

  public Dictionary<string, int> GetTimeZones(string ticketType, Func<int, string, string> zoneSelector)
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    filter.AddFilter(TechKasRequisites.TicketType, ticketType);
    filter.AddFilter(TechKasRequisites.TicketStatus, TicketStatus.Active);
    var calendar = new CalendarCalculator(this.repository); 
    
    var result = new Dictionary<string, int>
    {
      {"red", 0},
      {"yellow", 0},
      {"sandy", 0},
      {"green", 0}
    };

    foreach (var ticket in this.tickets)
    {
      Autoclicker.ClickYes();
      var detail = ticket.GetDetail(4);
      var record = detail.First();
      
      var start = DateTime.Now;
      var end = DateTime.Now;
      bool hasStart = false;
      bool hasEnd = false;
      int spentTime = 0;
      while (!detail.IsEndOfList())
      {
        if (record.GetRequisite(TechKasRequisites.TicketStatusDetail, RequisitesMode.AsString) == TicketStatus.InWork)
        {
          hasStart = true;
          start = DateTime.ParseExact(record.GetRequisite(TechKasRequisites.DateStatusDetail, RequisitesMode.AsString),
            "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }
        else if (hasStart && new[]{TicketStatus.OnControl, TicketStatus.Forwarded}
                   .Contains(record.GetRequisite(TechKasRequisites.TicketStatusDetail, RequisitesMode.AsString)))
        {
          hasEnd = true;
          end = DateTime.ParseExact(record.GetRequisite(TechKasRequisites.DateStatusDetail, RequisitesMode.AsString),
            "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }
        record = detail.Next();

        if (detail.IsEndOfList() && !hasEnd)
        {
          hasEnd = true;
          end = DateTime.Now;
        }

        if (hasStart && hasEnd)
        {
         spentTime += calendar.GetDifferenceInMinutes(start, end);
         hasStart = false;
         hasEnd = false;
        }
      }
      string ticketZone = zoneSelector(spentTime, ticket.GetRequisite(TechKasRequisites.Priority, RequisitesMode.AsString));
      result[ticketZone]++;
    }
    return result;
  }

  /// <summary>
  /// Получить зону для графика только на основе затраченного времени.
  /// </summary>
  /// <param name="timeInMinutes">Затраченное время в минутах.</param>
  /// <returns>Зона, к которой принадлежит обращение.</returns>
  private string GetColorZoneByTime(int timeInMinutes, string priority)
  {
    switch (timeInMinutes)
    {
      case int t when t <= 8 * 60:
        return "green";
      case int t when t <= 16 * 60:
        return "sandy";
      case int t when t <= 24 * 60:
        return "yellow";
      default:
        return "red";
    }
  }

  private string GetColorZoneBySLA(int timeInMinutes, string priority)
  {
    var priorities = this.repository.GetByPredicate<Priority>(x => true)
      .ToDictionary(p => p.Name, p => p.TimeToSolve);
    float spentSLATime = (float)timeInMinutes / priorities[priority];
    switch (spentSLATime)
    {
      case float t when t <= 0.25:
        return "green";
      case float t when t <= 0.5:
        return "sandy";
      case float t when t <= 0.75:
        return "yellow";
      default:
        return "red";
    }
  }

  /// <summary>
  /// Установить первоначальные фильтры из БД.
  /// </summary>
  private void SetInitFilters()
  {
    List<TechKasFilter> initFilters = this.repository.GetByPredicate<TechKasFilter>(
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
    var employees = this.team.Employees;
    var persNumbers = new List<string>();
    foreach (var employee in employees)
      persNumbers.Add(employee.TechKASNumber);
    this.tickets.SetFilter(TechKasRequisites.Employee, persNumbers);
  }
  
  #endregion
  
  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  public MetricCalculator(Team team)
  {
    this.team = team;
    this.repository = new DBRepository();
    this.tickets = new TechKasReference("ПДД");
    this.SetInitFilters();
    this.SetEmployeeFilters();
  }
  
  #endregion
}