using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

/// <summary>
///   Сервис команд.
/// </summary>
public class TeamService(IRepository repository) : GenericService(repository)
{
  /// <summary>
  ///   Получить список всех команд.
  /// </summary>
  /// <returns>Список всех команд.</returns>
  public async Task<List<Team>> GetAllTeams()
  {
    return await this.repository.GetAsync<Team>(x => true);
  }

  public async Task<Team> GetTeamById(int id)
  {
    return await this.repository.GetById<Team>(id);
  }
}