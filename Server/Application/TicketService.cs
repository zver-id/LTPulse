using CommonModels;
using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

public class TicketService
{
  private IRepository Repository { get; }
  
  public async Task<List<Ticket>> GetTickets(int teamId, DateTime date, string ticketType)
  {
    return await Task.Run(() =>
      {
        switch (ticketType)
        {
          case "Инциденты" or "Консультации":
            var totalDayMetric = this.Repository
              .Get<Metric>(m =>
                m.Team.Id == teamId && m.Date.Date == date.Date && m.MetricType.Name == MetricTypes.Tail)
              .FirstOrDefault();
            if (totalDayMetric == null)
              throw new ArgumentException("Расчет за дату не воспроизводился. Отображать нечего");
            return totalDayMetric.Tickets
              .Where(t => t.Type == ticketType)
              .ToList();
          default:
            try
            {
              return this.Repository.Get<Metric>(m =>
                  m.Date.Date == date.Date && m.Team.Id == teamId && m.MetricType.Name == ticketType)
                .First()
                .Tickets
                .ToList();
            }
            catch (InvalidOperationException ex)
            {
              throw new ArgumentException("Расчет за дату не воспроизводился. Отображать нечего", ex);
            }
        }
      }
    );
  }

  public TicketService(IRepository repository)
  {
    this.Repository = repository;
  }
}