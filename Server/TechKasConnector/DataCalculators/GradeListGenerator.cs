using System.Globalization;
using CommonModels.Interfaces;
using CommonModels.Models;
using TechKasConnector.Calendar;
using TechKasConnector.Requisites;

namespace TechKasConnector.DataCalculators;

/// <summary>
/// Генератор списка оценок.
/// </summary>
public class GradeListGenerator
{
  /// <summary>
  /// Список оценок.
  /// </summary>
  public List<Grade> Grades { get; } = new();
 
  /// <summary>
  /// Команда по которой ведется расчет.
  /// </summary>
  private Team Team { get; set; }
  
  /// <summary>
  /// Репозиторий.
  /// </summary>
  private IRepository Repository { get; }
  /// <summary>
  /// Календарь рабочего времени.
  /// </summary>
  private CalendarCalculator Calendar { get; }
  
  /// <summary>
  /// Логгер.
  /// </summary>
  private readonly ILogger<GradeListGenerator> logger;
  
  /// <summary>
  /// Рассчитать список оценок.
  /// </summary>
  private void CalculateGrades()
  {
    this.logger.LogInformation("Расчет оценок");
    var gradesReference = new TechKasReference("REQUEST_SOLUTION_MARKS", false);
    var currentDate = this.Calendar.GetPreviousDates(0);
    
    using var filter = new ReferenceFilterManager(gradesReference);
    filter.AddFilter(TechKasRequisites.ClosedDate, currentDate);
    var listOfEmployeeNames = this.Team.Employees
      .Select(e => e.Name).ToList();

    foreach (var grade in gradesReference)
    {
      try
      {
        Grade newGrade = this.GetOrCreateGrade(grade);
        this.Repository.Add(newGrade);
        if (listOfEmployeeNames.Contains(newGrade.Ticket.Employee))
          this.Grades.Add(newGrade);
      }
      catch (ArgumentNullException e)
      {
        this.logger.LogError(e, "Не найдено связанное с оценкой обращение. Оценка пропущена");
        continue;
      }
    }
  }
  
  /// <summary>
  /// Создать или получить существующую оценку.
  /// </summary>
  /// <param name="element">Элемент ТехКас.</param>
  /// <returns>Оценка.</returns>
  private Grade GetOrCreateGrade(TechKasElement element)
  {
    var id = int.Parse(element.GetRequisite(TechKasRequisites.GradeTicketNum, RequisitesMode.AsString).Trim());
    var grade = this.Repository.GetById<Grade>(id);
    if (grade != null)
      return grade;
    return new Grade
    {
      Id = id,
      Score = int.Parse(element.GetRequisite(TechKasRequisites.GradeScore, RequisitesMode.AsString)),
      Text = element.GetRequisiteWithOpen(TechKasRequisites.GradeText, RequisitesMode.AsString),
      Date = DateTime.ParseExact(element.GetRequisite(TechKasRequisites.GradeDate, RequisitesMode.AsString),
        "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
      Ticket = this.GetRelatedTicket(id)
    };
  }

  /// <summary>
  /// Получить связанное обращение.
  /// </summary>
  /// <param name="ticketNumber">Номер обращения.</param>
  /// <returns>Связанное обращение.</returns>
  /// <exception cref="ArgumentNullException">Возникает, когда не удается найти обращение по указанному ИД.</exception>
  private Ticket GetRelatedTicket(int ticketNumber)
  {
    var ticket = this.Repository.GetById<Ticket>(ticketNumber);
    if (ticket != null)
      return ticket;
    var ticketReference = new TechKasReference("ПДД", false);
    using var filter = new ReferenceFilterManager(ticketReference);
    // В ТехКас все номера обращений имеют 4 пробела в начале. Без этого не фильтруется.
    filter.AddFilter(TechKasRequisites.Id,$"    {ticketNumber}");
    var ticketElement = ticketReference.FirstOrDefault();
    if (ticketElement == null)
    {
      this.logger.LogError($"Переданный ИД {ticketNumber} обращения не существует в ТехКас");
      throw new ArgumentNullException($"Переданный ИД {ticketNumber} обращения не существует в ТехКас");
    }
    ticket = new Ticket
    {
      Id = int.Parse(ticketElement.GetRequisite(TechKasRequisites.Id, RequisitesMode.AsString).Trim()),
      Name = ticketElement.GetRequisite(TechKasRequisites.Name, RequisitesMode.AsString),
      Type = ticketElement.GetRequisite(TechKasRequisites.TicketType, RequisitesMode.AsString),
      Organization = ticketElement.GetRequisite(TechKasRequisites.Organization, RequisitesMode.DisplayText),
      Employee = ticketElement.GetRequisite(TechKasRequisites.Employee, RequisitesMode.DisplayText),
      Priority = this.Repository
        .Get<Priority>(x =>
          x.Name == ticketElement.GetRequisite(TechKasRequisites.Priority, RequisitesMode.AsString)).First(),
      IncomingDate = DateTime.ParseExact(
        ticketElement.GetRequisite(TechKasRequisites.OpenDate, RequisitesMode.AsString),
        "dd.MM.yyyy", CultureInfo.InvariantCulture),
      State = this.Repository.Get<TicketState>(s =>
        s.State == ticketElement.GetRequisite(TechKasRequisites.TicketStatus, RequisitesMode.AsString)).First(),
      TimeInWork = 0,
      Hyperlink = ticketElement.Hyperlink
    };
    this.Repository.Add(ticket);
    return ticket;
  }

  /// <summary>
  /// Рассчитать список оценок для команды.
  /// </summary>
  /// <param name="team">Команда.</param>
  /// <returns>Список оценок.</returns>
  public List<Grade> GenerateForTeam(Team team)
  {
    this.Team = team;
    this.CalculateGrades();
    return this.Grades;
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="logger">Логгер.</param>
  /// <param name="repository">Репозиторий.</param>
  /// <param name="calendar">Календарь рабочего времени.</param>
  public GradeListGenerator(ILogger<GradeListGenerator> logger, IRepository repository, CalendarCalculator calendar)
  {
    this.logger = logger;
    this.Repository = repository;
    this.Calendar = calendar;
  }
}