using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

/// <summary>
/// Сервис статистики по сотрудникам.
/// </summary>
public class EmployeeStatsService(IRepository repository, TeamService teamService)
{
  /// <summary>
  /// Состояние обращения, означающее закрытие.
  /// </summary>
  private const string ClosedState = "Закрыто";

  /// <summary>
  /// Порог возраста обращения в днях для метрики "старше 2 недель".
  /// </summary>
  private const int TwoWeeksDays = 14;

  /// <summary>
  /// Порог возраста обращения в днях для метрики "старше 3 недель".
  /// </summary>
  private const int ThreeWeeksDays = 21;

  /// <summary>
  /// Порог возраста обращения в днях для метрики "старше месяца".
  /// </summary>
  private const int OneMonthDays = 28;

  /// <summary>
  /// Граница критической зоны по времени в работе в часах.
  /// </summary>
  private const float TimeCritical = 24f;

  /// <summary>
  /// Граница оранжевой зоны по времени в работе в часах.
  /// </summary>
  private const float TimeOrange = 16f;

  /// <summary>
  /// Граница жёлтой зоны по времени в работе в часах.
  /// </summary>
  private const float TimeYellow = 8f;

  /// <summary>
  /// Значение оценки, считающееся максимальной.
  /// </summary>
  private const int GoodScore = 4;

  /// <summary>
  /// Порог количества ответов обращения, считающегося нормой для метрики "Кол-во ответов_%".
  /// </summary>
  private const int AnswerCountLimit = 2;

  /// <summary>
  /// Репозиторий.
  /// </summary>
  private readonly IRepository repository = repository;

  /// <summary>
  /// Сервис команд.
  /// </summary>
  private readonly TeamService teamService = teamService;

  /// <summary>
  /// Рассчитать статистику по каждому сотруднику команды за период.
  /// </summary>
  /// <param name="teamId">Идентификатор команды.</param>
  /// <param name="beginDate">Начало периода (включительно).</param>
  /// <param name="endDate">Конец периода (включительно).</param>
  /// <returns>Список статистики по сотрудникам. Пустой список, если команда не найдена.</returns>
  public async Task<List<EmployeeStats>> GetStats(int teamId, DateTime beginDate, DateTime endDate)
  {
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return new List<EmployeeStats>();

    var tickets = await this.repository.GetAsync<Ticket>(t => t.IncomingDate >= beginDate && t.IncomingDate < endDate.AddDays(1));
    var grades = await this.repository.GetAsync<Grade>(g => g.Date >= beginDate && g.Date < endDate.AddDays(1));

    var teamEmployeeNames = team.Employees.Select(e => e.Name).ToHashSet();
    var teamTickets = tickets.Where(t => teamEmployeeNames.Contains(t.Employee)).ToList();
    var teamGrades = grades.Where(g => g.Ticket != null && teamEmployeeNames.Contains(g.Ticket.Employee)).ToList();

    return team.Employees
      .Select(e => BuildStats(e.Name,
        teamTickets.Where(t => t.Employee == e.Name).ToList(),
        teamGrades.Where(g => g.Ticket?.Employee == e.Name).ToList(),
        DateTime.Now))
      .ToList();
  }

  /// <summary>
  /// Сформировать статистику для одного сотрудника по его обращениям и оценкам.
  /// </summary>
  /// <param name="name">Имя сотрудника.</param>
  /// <param name="tickets">Обращения сотрудника за период.</param>
  /// <param name="grades">Оценки по обращениям сотрудника за период.</param>
  /// <param name="asOf">Отсчётная дата, от которой считается возраст обращения (для открытых обращений — текущая дата).</param>
  /// <returns>Статистика сотрудника.</returns>
  private static EmployeeStats BuildStats(string name, List<Ticket> tickets, List<Grade> grades, DateTime asOf)
  {
    var closed = tickets.Where(t => string.Equals(t.State?.State, ClosedState, StringComparison.Ordinal));
    var open = tickets.Where(t => !string.Equals(t.State?.State, ClosedState, StringComparison.Ordinal));
    var withTime = tickets.Where(t => t.TimeInWork > 0).ToList();
    var withSla = tickets.Where(t => t.Priority != null && t.Priority.TimeToSolve > 0).ToList();
    var withReaction = tickets
      .Where(t => t.Priority != null && t.Priority.TimeToReaction > 0 && t.TimeToFirstResponse > 0)
      .ToList();

    Func<Ticket, DateTime> endOfTicket = t => t.ClosingDate ?? asOf;

    return new EmployeeStats
    {
      Employee = name,
      Assigned = tickets.Count,
      Closed = closed.Count(),
      Backlog = open.Count(),
      AvgResolutionTime = withTime.Count > 0 ? MathF.Round(withTime.Average(t => t.TimeInWork), 1) : 0,
      SlaResolution = withSla.Count > 0
        ? MathF.Round(100f * withSla.Count(t => t.TimeInWork / t.Priority.TimeToSolve <= 1f) / withSla.Count, 1)
        : null,
      SlaReaction = withReaction.Count > 0
        ? MathF.Round(100f * withReaction.Count(t => t.TimeToFirstResponse / t.Priority.TimeToReaction <= 1f) / withReaction.Count, 1)
        : null,
      ResponseCount = tickets.Count > 0
        ? MathF.Round(100f * tickets.Count(t => t.AnswerCount <= AnswerCountLimit) / tickets.Count, 1)
        : null,
      EscalationLine = tickets.Count > 0
        ? MathF.Round(100f * tickets.Count(t => !string.IsNullOrEmpty(t.LineEscalationsData)) / tickets.Count, 1)
        : null,
      EscalationDevs = tickets.Count > 0
        ? MathF.Round(100f * tickets.Count(t => !string.IsNullOrEmpty(t.DevsEscalationsData)) / tickets.Count, 1)
        : null,
      CriticalZones = tickets.Count(t => t.TimeInWork >= TimeCritical),
      OrangeZones = tickets.Count(t => t.TimeInWork >= TimeOrange && t.TimeInWork < TimeCritical),
      YellowZones = tickets.Count(t => t.TimeInWork >= TimeYellow && t.TimeInWork < TimeOrange),
      GreenZones = tickets.Count(t => t.TimeInWork < TimeYellow),
      Older2Weeks = tickets.Count(t => (endOfTicket(t) - t.IncomingDate).TotalDays >= TwoWeeksDays),
      Older3Weeks = tickets.Count(t => (endOfTicket(t) - t.IncomingDate).TotalDays >= ThreeWeeksDays),
      Older1Month = tickets.Count(t => (endOfTicket(t) - t.IncomingDate).TotalDays >= OneMonthDays),
      GradeScore = grades.Count > 0
        ? MathF.Round(100f * grades.Count(g => g.Score == GoodScore) / grades.Count, 1)
        : null
    };
  }
}
