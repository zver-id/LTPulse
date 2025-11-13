namespace TechKasConnectService;

public class MetricsCalculatorService : BackgroundService
{
  private readonly ILogger<MetricsCalculatorService> logger;



  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      if (logger.IsEnabled(LogLevel.Information))
      {
        logger.LogInformation("MetricsCalculatorService running at: {time}", DateTimeOffset.Now);
      }

      await Task.Delay(1000, stoppingToken);
    }
  }
  
  public MetricsCalculatorService(ILogger<MetricsCalculatorService> logger)
  {
    this.logger = logger;
  }
}