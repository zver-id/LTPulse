using System.Globalization;
using CommonModels.Models;
using DBCore;
using TechKasConnector.Requisites;

namespace TechKasConnector;

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
    int ticketFilter = this.tickets.SetFilter(TechKasRequisites.TicketType, type);
    int result = this.tickets.Count;
    this.tickets.DeleteFilter(ticketFilter);
    return result;
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
    var dateFilterId = this.tickets.SetFilter(TechKasRequisites.OpenDate, dateForCalculation);
    var result = GetTicketCountByType(type);
    this.tickets.DeleteFilter(dateFilterId);
    return result;
  }

  /// <summary>
  /// Количество обращений в работе по месяцам создания.
  /// </summary>
  /// <returns>Количество обращений в работе по месяцам создания.</returns>
  public Dictionary<string, int> GetCountTicketInProgressByMonth()
  {
    var result = new Dictionary<string, int>();
    int statusFilterId = this.tickets.SetFilter(TechKasRequisites.TicketStatus, "Р");
    
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
    this.tickets.DeleteFilter(statusFilterId);
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
    int ticketStatusFilter;
    if (activeOnly)
      ticketStatusFilter = this.tickets.SetFilter(TechKasRequisites.TicketStatus, TicketStatus.InWork);
    else
      ticketStatusFilter = this.tickets.SetFilter(TechKasRequisites.TicketStatus, TicketStatus.Active);
    var ticketTypeFilter = this.tickets.SetFilter(TechKasRequisites.TicketType, TicketType.WithoutProblems);
    // Убираем анонимки (Код-338)
    var excludeAnonims = this.tickets.SetFilter(TechKasRequisites.SupportArea, "30262732");
    int timeFilter = 0;
    if (daysAgo != -1)
    {
      timeFilter = this.tickets.SetFilter(TechKasRequisites.OpenDate,
        DateTime.Now.AddDays(-daysAgo).ToString("dd.MM.yyyy"), "<=");
    }

    var count = this.tickets.Count;
    this.tickets.DeleteFilter(ticketStatusFilter);
    this.tickets.DeleteFilter(excludeAnonims);
    this.tickets.DeleteFilter(ticketTypeFilter);
    if (timeFilter != 0)
      this.tickets.DeleteFilter(timeFilter);

    return count;
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
      .Where(f => f.ShouldInclude == true)
      .GroupBy(f => f.NameOfField)
      .ToDictionary(
        f => f.Key,
        f => f.Select(f=> f.Value).ToList()
      );
    foreach (var filter in includeFilters)
      this.tickets.SetFilter(filter.Key, filter.Value);
    
    Dictionary<string, List<string>> excludeFilters = initFilters
      .Where(f => f.ShouldInclude == false)
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