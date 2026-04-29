using CommonModels.Models;

namespace Application.RabbitMQRequests;

public class GenerateTeamReportRequest
{
  public int TeamId { get; set; }
  public int? DaysAgo { get; set; }
}