using CommonModels.Interfaces;
using CommonModels.Models;

namespace Application;

/// <summary>
/// Управление отчетами.
/// </summary>
public class GenerateReportService(IRepository repository) : GenericService(repository)
{
  public async Task<DateTime?> GetLastestGenerationTime(int teamId)
  {
    var teamJob = await this.repository.GetFirstAsync<Job>(j => j.Team.Id == teamId);
    return teamJob.StartProcess - teamJob.RepeatInterval;
  }
}