using Application;
using Common;

namespace TechKasConnectService;

public class MetricsCalculatorService : BackgroundService
{
  private readonly ILogger<MetricsCalculatorService> logger;
  private RabbitMQProducer rabbitMQProducer;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("MetricsCalculatorService running at: {time}", DateTimeOffset.Now);
    this.rabbitMQProducer = await RabbitMQProducer.CreateAsync(Settings.RabbitMQConnectionString);
    while (!stoppingToken.IsCancellationRequested)
    {

    }
  }
  
  public MetricsCalculatorService(ILogger<MetricsCalculatorService> logger)
  {
    this.logger = logger;
  }
}