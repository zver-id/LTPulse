using CommonModels.Models;
using DBCore;

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
        this.tickets.SetFilter(filter.Key, filter.Value.First(), false);
      }
      else
      {
        foreach (var value in filter.Value)
        {
          this.tickets.SetFilter(filter.Key, value, false);
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