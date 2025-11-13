using Common.Models;
using DBCore;

namespace Application;

/// <summary>
/// Сервис команд.
/// </summary>
public class TeamService
{
  /// <summary>
  /// Репозиторий.
  /// </summary>
  private readonly DBRepository repository;
  
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
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  public TeamService()
  {
    this.repository = new DBRepository();
  }
}