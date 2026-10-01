using DBCore;

namespace TechKasConnector.Queries;

/// <summary>
/// Исполнитель SQL-запросов к флагом расчета метрик задач.
/// Все запросы берутся из <see cref="JobCalculatingQueries"/> и выполняются
/// через единую сессию <see cref="IUnitOfWork"/> (с сериализацией операций).
/// </summary>
public class JobCalculatingQueryRepository
{
  private readonly IUnitOfWork unitOfWork;

  /// <summary>
  /// Атомарно «занимает» задачу: устанавливает <c>IsCalculating = true</c>,
  /// только если сейчас <c>false</c>.
  /// </summary>
  /// <param name="teamId">Ид команды.</param>
  /// <returns><c>true</c>, если задача была свободна и успешно занята, иначе <c>false</c>.</returns>
  public async Task<bool> ClaimJobAsync(int teamId)
  {
    var updated = await this.unitOfWork.ExecuteOnSessionAsync(session =>
      session.CreateSQLQuery(JobCalculatingQueries.ClaimJob)
        .SetParameter("teamId", teamId)
        .ExecuteUpdateAsync());
    return updated > 0;
  }

  /// <summary>
  /// Сбрасывает флаг <c>IsCalculating</c> у задачи команды (после окончания расчета).
  /// </summary>
  /// <param name="teamId">Ид команды.</param>
  public async Task ResetJobAsync(int teamId)
  {
    await this.unitOfWork.ExecuteOnSessionAsync(session =>
      session.CreateSQLQuery(JobCalculatingQueries.ResetJob)
        .SetParameter("teamId", teamId)
        .ExecuteUpdateAsync());
  }

  /// <summary>
  /// Сбрасывает флаг <c>IsCalculating</c> у всех задач (при старте сервиса).
  /// </summary>
  public Task ResetAllAsync()
  {
    return this.unitOfWork.ExecuteOnSessionAsync(session =>
      session.CreateSQLQuery(JobCalculatingQueries.ResetAll)
        .ExecuteUpdateAsync());
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="unitOfWork">Единица работы (владелец сессии).</param>
  public JobCalculatingQueryRepository(IUnitOfWork unitOfWork)
  {
    this.unitOfWork = unitOfWork;
  }
}
