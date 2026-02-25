using System.Globalization;
using System.Runtime.CompilerServices;
using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;
using TechKasConnector.Calendar;
using TechKasConnector.Requisites;

namespace TechKasConnector.DataCalculators;

/// <summary>
/// Класс для работы с "цветными" зонами.
/// </summary>
public class TicketListGenerator
{
  #region  Поля и свойства
  
  /// <summary>
  /// Список обращений.
  /// </summary>
  public List<Ticket> Tickets { get; init; } = new();
  
  /// <summary>
  /// Команда для которой идет расчет.
  /// </summary>
  private Team Team {get; init;}

  /// <summary>
  /// Репозиторий.
  /// </summary>
  private readonly IRepository repository;
  
  /// <summary>
  /// Справочник обращений.
  /// </summary>
  private readonly TechKasReference tickets;
  
  /// <summary>
  /// Календарь рабочего времени.
  /// </summary>
  private CalendarCalculator Calendar {get; init;}
  
  #endregion

  /// <summary>
  /// Добавить обращение к списку.
  /// </summary>
  /// <param name="ticket"></param>
  private void AddToTickets(Ticket ticket)
  {
    this.Tickets.Add(ticket);
    this.repository.AddOrUpdate(ticket);
  }

  /// <summary>
  /// Получить список обращений с вычисленным временем в работе.
  /// </summary>
  /// <param name="ticketType">Тип обращений.</param>
  private void GetTicketList(string? ticketType = null)
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    if (ticketType != null)
      filter.AddFilter(TechKasRequisites.TicketType, ticketType);
    
    filter.AddFilter(TechKasRequisites.TicketStatus, TicketStatus.Active);
    
    foreach (var ticket in this.tickets)
    {
      var spentTime = this.GetSpentTimeByMinutes(ticket);
      var stampedTime = this.GetTimeStamp(ticket);
      
      var ticketRecord = this.GetTicket(ticket);
      ticketRecord.TimeInWork = (float)spentTime / 60;
      ticketRecord.TimeStampedOnDay = stampedTime;
      this.AddToTickets(ticketRecord);
    }
  }
  
  /// <summary>
  /// Рассчитать время, затраченное на обращение в минутах.
  /// </summary>
  /// <param name="ticket">Элемент обращения ТехКас.</param>
  /// <returns>Затраченное время в минутах.</returns>
  private int GetSpentTimeByMinutes(TechKasElement ticket)
  {
    Autoclicker.ClickYes();
    var detail = ticket.GetDetail(4);
    var record = detail.First();
      
    DateTime startOfIteration = DateTime.Now;
    DateTime endOfIteration = DateTime.Now;
    bool hasStart = false;
    bool hasEnd = false;
    int spentTime = 0;
    
    while (!detail.IsEndOfList())
    {
      if (record.GetRequisite(TechKasRequisites.TicketStatusDetail, RequisitesMode.AsString) == "В работе")
      {
        hasStart = true;
        startOfIteration = DateTime.ParseExact(record.GetRequisite(TechKasRequisites.DateStatusDetail, RequisitesMode.AsString),
          "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
      }
      else if (hasStart && new[]{"На контроле", "Переадресовано"}
                 .Contains(record.GetRequisite(TechKasRequisites.TicketStatusDetail, RequisitesMode.AsString)))
      {
        hasEnd = true;
        endOfIteration = DateTime.ParseExact(record.GetRequisite(TechKasRequisites.DateStatusDetail, RequisitesMode.AsString),
          "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
      }
      record = detail.Next();

      if (detail.IsEndOfList() && !hasEnd)
      {
        hasEnd = true;
        endOfIteration = DateTime.Now;
      }

      if (hasStart && hasEnd)
      {
        spentTime += this.Calendar.GetDifferenceInMinutes(startOfIteration, endOfIteration);
        hasStart = false;
        hasEnd = false;
      }
    }
    return spentTime;
  }

  /// <summary>
  /// Считает количество времени, отмеченное за текущий день командой.
  /// </summary>
  /// <param name="ticket">Обращение у которого считаем отмеченное время.</param>
  /// <param name="daysAgo">Количество дней назад, за которое нужно считать.</param>
  /// <returns>Количество затраченного времени.</returns>
  private float GetTimeStamp(TechKasElement ticket, int daysAgo = 0)
  {
    float total = 0;
    var employeeNames = this.Team.Employees.Select(e => e.Name).ToList();
    
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
        total += float.Parse(record.GetRequisite(TechKasRequisites.TimeSpent, RequisitesMode.AsString),
          CultureInfo.InvariantCulture);
      }
    }
    return total;
  }

  /// <summary>
  /// Получить обращение из элемента ТехКас.
  /// </summary>
  /// <param name="element">Элемент ТехКас.</param>
  /// <returns>Обращение.</returns>
  private Ticket GetTicket(TechKasElement element)
  {
     return new Ticket
      {
        Id = int.Parse(element.GetRequisite(TechKasRequisites.Id, RequisitesMode.AsString).Trim()),
        Name = element.GetRequisite(TechKasRequisites.Name, RequisitesMode.AsString),
        Type = element.GetRequisite(TechKasRequisites.TicketType, RequisitesMode.AsString),
        Organization = element.GetRequisite(TechKasRequisites.Organization, RequisitesMode.DisplayText),
        Employee = element.GetRequisite(TechKasRequisites.Employee, RequisitesMode.DisplayText),
        Priority = this.repository
          .Get<Priority>(x =>
            x.Name == element.GetRequisite(TechKasRequisites.Priority, RequisitesMode.AsString)).First(),
        IncomingDate = DateTime.ParseExact(element.GetRequisite(TechKasRequisites.OpenDate, RequisitesMode.AsString),
          "dd.MM.yyyy", CultureInfo.InvariantCulture),
        State = this.repository.Get<TicketState>(s =>
          s.State == element.GetRequisite(TechKasRequisites.TicketStatus, RequisitesMode.AsString)).First(),
        TimeInWork = 0,
        Hyperlink = element.Hyperlink
      };
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="repository">Репозиторий.</param>
  /// <param name="tickets">Справочник обращений.</param>
  public TicketListGenerator(IRepository repository, TechKasReference tickets, Team team, string? ticketType = null)
  {
    this.repository = repository;
    this.tickets = tickets;
    this.Team = team;
    this.Calendar = new CalendarCalculator(this.repository);
    this.GetTicketList(ticketType);
  }
}