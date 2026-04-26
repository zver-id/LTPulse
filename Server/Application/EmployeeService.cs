using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

public class EmployeeService(IRepository repository) : GenericService(repository)
{
  public async Task<List<Employee>> GetEmployeesByTeamId(int teamId)
  {
    Team team = await this.repository.GetById<Team>(teamId);
    return team.Employees.ToList();
  }
}