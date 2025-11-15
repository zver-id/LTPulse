using CommonModels.Models;
using DBCore;

namespace Application;

/// <summary>
/// Сервис команд.
/// </summary>
public class TeamService : GenericService
{
  /// <summary>
  /// Получить список всех команд.
  /// </summary>
  /// <returns>Список всех команд.</returns>
  public Task<List<Team>> GetAllTeams()
  {
    return Task.Run(() =>
      {
        return this.repository.GetByPredicate<Team>(x => true);
      }
    );
  }

  public Task<Team> GetTeamById(int id)
  {
    return Task.Run(() =>
      {
        return this.repository.GetById<Team>(id);
      }
    );
  }
}