using Application;
using Application.RabbitMQRequests;
using CommonModels.Interfaces;
using CommonModels.Models;
using TechKasConnector.Calendar;

namespace TechKasConnector;

/// <summary>
/// Планировщик.
/// </summary>
public class SchedulerService : BackgroundService
{
  private ILogger<SchedulerService> logger;
  private IServiceScopeFactory serviceScopeFactory;
  
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    this.logger.LogInformation("Scheduler Service is starting.");
    using var scope = this.serviceScopeFactory.CreateScope();
    var scheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();
    scheduler.CreateJobsForNewTeams();

    while (!stoppingToken.IsCancellationRequested)
    {
      this.logger.LogInformation("Inspect job to start.");
      await scheduler.StartJobs();
      await Task.Delay(TimeSpan.FromMinutes(5));
    }
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="logger"></param>
  /// <param name="serviceScopeFactory"></param>
  /// <param name="configuration"></param>
  /// <param name="repository"></param>
  /// <param name="calendar"></param>
  public SchedulerService(ILogger<SchedulerService> logger, IServiceScopeFactory serviceScopeFactory)
  {
    this.logger = logger;
    this.serviceScopeFactory = serviceScopeFactory;
  }
}