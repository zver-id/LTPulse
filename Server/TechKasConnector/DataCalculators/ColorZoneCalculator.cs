using System.Globalization;
using System.Runtime.CompilerServices;
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
  public List<Ticket> Tickets { get; init; }
  
  /// <summary>
  /// Получить количество обращений по зонам с учётом времени.
  /// </summary>
  public Dictionary<string, int> ColorZonesByTime =>
    this.Tickets.Select(ticket => this.GetColorZoneByTime(ticket.TimeInWork))
      .GroupBy(p=>p)
      .ToDictionary(p=>p.Key,p=>p.Count());
  
  /// <summary>
  /// Получить количество обращений по зонам с учётом приоритета.
  /// </summary>
  public Dictionary<string, int> ColorZonesByPriority =>
    this.Tickets.Select(ticket => this.GetColorZoneBySLA(ticket.TimeInWork, ticket.Priority.Name))
      .GroupBy(p=>p)
      .ToDictionary(p=>p.Key,p=>p.Count());
  
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

  /// <summary>
  /// Получить список обращений с вычисленным временем в работе.
  /// </summary>
  /// <param name="ticketType">Тип обращений.</param>
  private void GetTicketList(string ticketType)
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
      ticketRecord.TimeInWork = (float)spentTime / 60;
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
  /// <param name="timeInMinutes">Затраченное время в часах.</param>
  /// <returns>Зона, к которой принадлежит обращение.</returns>
  private string GetColorZoneByTime(float timeInMinutes)
  {
    switch (timeInMinutes)
    {
      case <= 8:
        return "green";
      case <= 16:
        return "sandy";
      case <= 24:
        return "yellow";
      default:
        return "red";
    }
  }

  /// <summary>
  /// Получить зону с учётом приоритета.
  /// </summary>
  /// <param name="timeInHours">Время в часах.</param>
  /// <param name="priority">Приоритет.</param>
  /// <returns>Цвет зоны строкой.</returns>
  private string GetColorZoneBySLA(float timeInHours, string priority)
  {
    var priorities = this.repository.GetByPredicate<Priority>(x => true)
      .ToDictionary(p => p.Name, p => p.TimeToSolve);
    float spentSLATime = (float)timeInHours / priorities[priority];
    switch (spentSLATime)
    {
      case var t when t <= 0.25:
        return "green";
      case var t when t <= 0.5:
        return "sandy";
      case var t when t <= 0.75:
        return "yellow";
      default:
        return "red";
    }
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="repository">Репозиторий.</param>
  /// <param name="tickets">Справочник обращений.</param>
  public ColorZoneCalculator(DBRepository? repository, TechKasReference tickets, string ticketType)
  {
    this.repository = repository;
    this.tickets = tickets;
    this.GetTicketList(ticketType);
  }
}