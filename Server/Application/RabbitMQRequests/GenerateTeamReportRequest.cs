using CommonModels.Models;

namespace Application.RabbitMQRequests;

public class GenerateTeamReportRequest
{
  public Team team;
  public int? daysAgo;
}