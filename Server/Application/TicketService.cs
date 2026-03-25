using CommonModels;
using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

public class TicketService
{
  private IRepository Repository { get; }
  
  public List<Ticket> GetTickets(int teamId, DateTime date, string ticketType)
  {
    switch (ticketType)
    {
      case ("incidents"):
        var totalDayMetric = this.Repository
          .Get<Metric>(m => m.Team.Id == teamId && m.Date.Date == date.Date && m.MetricType.Name == MetricTypes.Tail)
          .FirstOrDefault();
        if (totalDayMetric == null)
          throw new ArgumentException("Расчет за дату не воспроизводился. Отображать нечего");
        return totalDayMetric.Tickets
          .Where(t => t.Type == "Инцидент")
          .ToList();
      default:
        throw new ArgumentException("Неизвестный параметр типа обращений");
    }
  }

  public TicketService(IRepository repository)
  {
    this.Repository = repository;
  }
}