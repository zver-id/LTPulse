using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

public class GradeService(IRepository repository) : GenericService(repository)
{
  /// <summary>
  /// Получить все негативные оценки.
  /// </summary>
  /// <param name="team">Команда.</param>
  /// <param name="onlyUnresearched">Только непроработанные.</param>
  /// <returns>Список оценок.</returns>
  public async Task<List<Grade>> GetNegativeGrades(Team team, bool onlyUnresearched)
  {
    var employees = team.Employees
      .Select(employee => employee.Name);
    return await this.repository.GetAsync<Grade>(grade => 
      grade.Score == 2 && 
      (!onlyUnresearched || grade.isResearched == false) && 
      employees.Contains(grade.Ticket.Employee));
  }

  /// <summary>
  /// Получить все оценки за период.
  /// </summary>
  /// <param name="team">Команда.</param>
  /// <param name="startDate">Начало периода.</param>
  /// <param name="endDate">Конец периода.</param>
  /// <returns>Список оценок.</returns>
  public async Task<List<Grade>> GetAllGrades(Team team, DateTime startDate, DateTime endDate)
  {
    var employees = team.Employees
      .Select(employee => employee.Name);
    return await this.repository.GetAsync<Grade>(grade =>
      employees.Contains(grade.Ticket.Employee) && 
      grade.Date.Date <= startDate.Date && grade.Date.Date >= endDate.Date);
  }
}