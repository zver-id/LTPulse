using CommonModels.Models;

namespace Application.RabbitMQRequests;

public class GenerateTeamReportRequest
{
  public Team Team { get; set; }
  public int? DaysAgo { get; set; }
}