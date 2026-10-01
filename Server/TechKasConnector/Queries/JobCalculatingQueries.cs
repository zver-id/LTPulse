namespace TechKasConnector.Queries;

/// <summary>
/// SQL-запросы, связанные с флагом расчета метрик у задачи (<see cref="CommonModels.Models.Job.IsCalculating"/>).
/// Вынесены из кода для читаемости и централизованного управления.
/// </summary>
public static class JobCalculatingQueries
{
  /// <summary>
  /// Атомарно «занимает» задачу: устанавливает <c>IsCalculating = true</c> только
  /// если сейчас <c>false</c>. Возвращает количество измененных строк
  /// (0 — задача уже занята, 1 — успешно занята).
  /// </summary>
  public const string ClaimJob =
    "UPDATE \"Job\" SET \"iscalculating\" = true WHERE team_id = :teamId AND \"iscalculating\" = false";

  /// <summary>
  /// Сбрасывает флаг <c>IsCalculating</c> у задачи команды после окончания расчета.
  /// </summary>
  public const string ResetJob =
    "UPDATE \"Job\" SET \"iscalculating\" = false WHERE team_id = :teamId";

  /// <summary>
  /// Сбрасывает флаг <c>IsCalculating</c> у всех задач (вызывается при старте сервиса,
  /// чтобы после аварийного завершения не отбрасывались все сообщения).
  /// </summary>
  public const string ResetAll =
    "UPDATE \"Job\" SET \"iscalculating\" = false";
}
