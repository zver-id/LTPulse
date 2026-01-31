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
  private readonly ILogger logger;
  
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
      Grade newGrade = this.GetOrCreateGrade(grade);
      this.Repository.AddOrUpdate(newGrade);
      if (listOfEmployeeNames.Contains(newGrade.Ticket.Employee))
        this.Grades.Add(newGrade);
    }
  }
  
  /// <summary>
  /// Создать или получить сущетсвующую оценку.
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
      Text = element.GetRequisite(TechKasRequisites.GradeText, RequisitesMode.AsString),
      Date = DateTime.ParseExact(element.GetRequisite(TechKasRequisites.GradeDate, RequisitesMode.AsString),
        "dd.MM.yyyy", CultureInfo.InvariantCulture),
      Ticket = this.Repository.GetById<Ticket>(id)
    };
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
  public GradeListGenerator(ILogger logger, IRepository repository, CalendarCalculator calendar)
  {
    this.logger = logger;
    this.Repository = repository;
    this.Calendar = calendar;
  }
}