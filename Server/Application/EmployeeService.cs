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

  public async Task AddEmployeeToTeam(Employee employee, int teamId)
  {
    var team = await this.repository.GetById<Team>(teamId);
    if (team == null)
    {
      throw new ArgumentException("Team not found");
    }

    if (team.Employees.All(e => e.Id != employee.Id))
    {
      team.Employees.Add(employee);
    }
    await this.repository.Update(team);
  }
}