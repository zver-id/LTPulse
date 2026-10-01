using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

/// <summary>
/// Сервис работы с сотрудниками.
/// </summary>
public class EmployeeService(IRepository repository) : GenericService(repository)
{
  /// <summary>
  /// Получить сотрудников команды по её ID.
  /// </summary>
  /// <param name="teamId">ID команды</param>
  /// <returns>Список сотрудников команды.</returns>
  public async Task<List<Employee>> GetEmployeesByTeamId(int teamId)
  {
    Team team = await this.repository.GetById<Team>(teamId);
    return team.Employees.ToList();
  }

  /// <summary>
  /// Получить всех сотрудников (для админского выбора при включении в команду).
  /// </summary>
  /// <returns>Список всех сотрудников.</returns>
  public async Task<List<Employee>> GetAllEmployees()
  {
    return await this.repository.GetAsync<Employee>(_ => true);
  }

  public async Task AddEmployeeToTeam(Employee employee, int teamId)
  {
    var team = await this.repository.GetById<Team>(teamId);
    if (team == null)
    {
      throw new ArgumentException("Team not found");
    }

    // Обновляем Employee.Teams (с каскадом SaveUpdate), а не Team.Employees
    // (без каскада) — только так NHibernate обновит таблицу Employee_Teams.
    var existing = await this.repository.GetById<Employee>(employee.Id);
    if (existing == null)
    {
      throw new ArgumentException("Employee not found");
    }

    if (existing.Teams.All(t => t.Id != teamId))
    {
      existing.Teams.Add(team);
      await this.repository.Update(existing);
    }
  }

  /// <summary>
  /// Исключить сотрудника из команды.
  /// </summary>
  /// <param name="employeeId">Идентификатор сотрудника.</param>
  /// <param name="teamId">Идентификатор команды.</param>
  public async Task RemoveEmployeeFromTeam(int employeeId, int teamId)
  {
    var team = await this.repository.GetById<Team>(teamId);
    if (team == null)
    {
      throw new ArgumentException("Team not found");
    }

    var employee = await this.repository.GetById<Employee>(employeeId);
    if (employee == null)
    {
      throw new ArgumentException("Employee not found");
    }

    var teamInEmployee = employee.Teams.FirstOrDefault(t => t.Id == teamId);
    if (teamInEmployee == null)
    {
      throw new ArgumentException("Employee is not in this team");
    }

    employee.Teams.Remove(teamInEmployee);
    await this.repository.Update(employee);
  }
}