using System.Globalization;
using CommonModels.Interfaces;
using CommonModels.Models;
using TechKasConnector.Calendar;
using TechKasConnector.Requisites;

namespace TechKasConnector.DataCalculators;

/// <summary>
/// Генератор списка обращений.
/// </summary>
internal class TicketListGenerator
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
  public async Task InitTicketList(string? ticketType = null, int daysAgo = 0)
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    if (ticketType != null)
      filter.AddFilter(TechKasRequisites.TicketType, ticketType);
    
    int activeStatusFilterId = filter.AddFilter(TechKasRequisites.TicketStatus, TicketStatus.Active);
    await this.AddTechKasElementsToTickets();
    
    filter.RemoveFilter(activeStatusFilterId);
    var previousDates = await this.Calendar.GetPreviousDates(daysAgo);
    filter.AddFilter(TechKasRequisites.ClosedDate, previousDates.First());
    await this.AddTechKasElementsToTickets();
  }

  /// <summary>
  /// Добавить обращения справочника в список.
  /// </summary>
  private async Task AddTechKasElementsToTickets()
  {
    foreach (var ticket in this.tickets)
    {
      var spentTime = await this.GetSpentTimeByMinutes(ticket);
      var stampedTime = this.GetTimeStamp(ticket);
      
      var ticketRecord = await this.GetTicket(ticket);
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
  private async Task<int> GetSpentTimeByMinutes(TechKasElement ticket)
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
        spentTime += await this.Calendar.GetDifferenceInMinutes(startOfIteration, endOfIteration);
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
  private async Task<Ticket> GetTicket(TechKasElement element)
  {
    var id = int.Parse(element.GetRequisite(TechKasRequisites.Id, RequisitesMode.AsString).Trim());
    
    Ticket ticket = await this.repository.GetById<Ticket>(id);
    var name = element.GetRequisite(TechKasRequisites.Name, RequisitesMode.AsString);
    var type = element.GetRequisite(TechKasRequisites.TicketType, RequisitesMode.AsString);
    var organization = element.GetRequisite(TechKasRequisites.Organization, RequisitesMode.DisplayText);
    var employee = element.GetRequisite(TechKasRequisites.Employee, RequisitesMode.DisplayText);
    var priority = await this.repository
      .GetFirstAsync<Priority>(x =>
        x.Name == element.GetRequisite(TechKasRequisites.Priority, RequisitesMode.AsString));
    var incomingDate = DateTime.ParseExact(element.GetRequisite(TechKasRequisites.OpenDate, RequisitesMode.AsString),
      "dd.MM.yyyy", CultureInfo.InvariantCulture);
    var state = await this.repository.GetFirstAsync<TicketState>(s =>
      s.State == element.GetRequisite(TechKasRequisites.TicketStatus, RequisitesMode.AsString));
    var timeInWork = 0;
    var hyperlink = element.Hyperlink;
    if (ticket == null)
      ticket = new Ticket();

    ticket.Id = id;
    ticket.Name = name;
    ticket.Type = type;
    ticket.Organization = organization;
    ticket.Employee = employee;
    ticket.Priority = priority;
    ticket.IncomingDate = incomingDate;
    ticket.State = state;
    ticket.TimeInWork = timeInWork;
    ticket.Hyperlink = hyperlink;
    return ticket;
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
  }
}