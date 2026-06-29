using CommonModels;
using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

/// <summary>
/// Сервис работы с обращениями.
/// </summary>
public class TicketService(IRepository repository) : GenericService(repository)
{
  /// <summary>
  /// Получить обращения.
  /// </summary>
  /// <param name="teamId">Id команды.</param>
  /// <param name="date">Дата рассчета.</param>
  /// <param name="ticketType">Тип обращений.</param>
  /// <exception cref="InvalidOperationException">Возникает в случае,
  /// если обращения за эту дату не рассчитывались.</exception>
  /// <returns>Список обращений.</returns>
  public async Task<List<Ticket>> GetTickets(int teamId, DateTime date, string ticketType)
  {
    switch (ticketType)
    {
      case "Инцидент" or "Консультация" or "Запрос на обслуживание":
        var totalDayMetric = await this.repository
          .GetFirstAsync<Metric>(m =>
            m.Team.Id == teamId && m.Date.Date == date.Date && m.MetricType.Name == MetricTypes.Tail);
        return totalDayMetric.Tickets
          .Where(t => t.Type == ticketType)
          .ToList();
      
      default:
        var targetMetric = await this.repository.GetFirstAsync<Metric>(m =>
          m.Date.Date == date.Date && m.Team.Id == teamId && m.MetricType.Name == ticketType);

        return targetMetric
          .Tickets
          .ToList();
    }
  }

  /// <summary>
  /// Добавить или обновить обращение.
  /// </summary>
  /// <param name="ticket">Обращение.</param>
  public async Task AddOrUpdateTicket(Ticket ticket)
  {
    await this.repository.AddOrUpdate(ticket);
  }
}