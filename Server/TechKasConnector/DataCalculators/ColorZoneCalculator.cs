using System.Globalization;
using CommonModels.Models;
using DBCore;
using TechKasConnector.Calendar;
using TechKasConnector.Requisites;

namespace TechKasConnector.DataCalculators;

/// <summary>
/// Класс для работы с "цветными" зонами.
/// </summary>
public class ColorZoneCalculator
{
  /// <summary>
  /// Список обращений.
  /// </summary>
  public List<Ticket> Tickets { get; private set; }
  
  /// <summary>
  /// Репозиторий.
  /// </summary>
  private readonly DBRepository repository;
  
  /// <summary>
  /// Справочник обращений.
  /// </summary>
  private readonly TechKasReference tickets;

  /// <summary>
  /// Добавить обращение к списку.
  /// </summary>
  /// <param name="ticket"></param>
  private void AddToTickets(Ticket ticket)
  {
    this.Tickets.Add(ticket);
    this.repository.AddOrUpdate(ticket);
  }

  public void GetTicketList(string ticketType)
  {
    using var filter = new ReferenceFilterManager(this.tickets);
    filter.AddFilter(TechKasRequisites.TicketType, ticketType);
    filter.AddFilter(TechKasRequisites.TicketStatus, TicketStatus.Active);
    var calendar = new CalendarCalculator(this.repository); 
    
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
      var ticketRecord = this.GetTicket(ticket);
      ticketRecord.TimeInWork = spentTime;
      this.AddToTickets(ticketRecord);
    }
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
        Organization = element.GetRequisite(TechKasRequisites.Organization, RequisitesMode.DisplayText),
        Employee = element.GetRequisite(TechKasRequisites.Employee, RequisitesMode.DisplayText),
        Priority = this.repository
          .GetByPredicate<Priority>(x =>
            x.Name == element.GetRequisite(TechKasRequisites.Priority, RequisitesMode.AsString)).First(),
        IncomingDate = DateTime.ParseExact(element.GetRequisite(TechKasRequisites.OpenDate, RequisitesMode.AsString),
          "dd.MM.yyyy", CultureInfo.InvariantCulture),
        State = this.repository.GetByPredicate<TicketState>(s =>
          s.State == element.GetRequisite(TechKasRequisites.TicketStatus, RequisitesMode.AsString)).First(),
        TimeInWork = 0,
        Hyperlink = element.Hyperlink
      };
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

  public ColorZoneCalculator(DBRepository? repository, TechKasReference tickets)
  {
    this.repository = repository;
    this.tickets = tickets;
  }
}