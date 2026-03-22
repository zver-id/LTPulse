using System.Configuration;
using Application;
using Application.RabbitMQRequests;
using CommonModels.Interfaces;
using CommonModels.Models;
using TechKasConnector.Calendar;

namespace TechKasConnector;

/// <summary>
/// Планировщик задач.
/// </summary>
public class JobScheduler
{
  #region Поля и свойства
  
  private IRepository Repository { get; }
  private IConfiguration Configuration { get; }
  private ILogger<JobScheduler> Logger { get; }
  private CalendarCalculator Calendar { get; }
  
  /// <summary>
  /// Список задач.
  /// </summary>
  private List<Job> Jobs { get; set; }

  #endregion

  #region Методы

  /// <summary>
  /// Создает задачи для новых команд.
  /// </summary>
  public void CreateJobsForNewTeams()
  {
    this.Logger.LogInformation("Create jobs for new teams.");
    var teams = this.Repository.Get<Team>(t => true);
    var jobs = this.Repository.Get<Job>(j => true);
    foreach (var team in teams)
    {
      if (jobs.All(j => j.Team.Id != team.Id))
      {
        var newJob = new Job
        {
          Name = $"generate_report_{team.Name}",
          Team = team,
          StartProcess = DateTime.Now,
          RepeatInterval = TimeSpan.FromDays(1)
        };
        this.Repository.AddOrUpdate(newJob);
        this.Jobs.Add(newJob);
        this.Logger.LogInformation($"Create job for team {team.Name}");
      }
    }
  }

  /// <summary>
  /// Запустить все задачи.
  /// </summary>
  public async Task StartJobs()
  {
    string? rabbitMqConnectionString = this.Configuration.GetConnectionString("RabbitMQ");
    if (rabbitMqConnectionString == null)
      throw new ConfigurationErrorsException("RabbitMQ connection string not found");
    var rabbitMqClient = await RabbitMQClient.CreateAsync(rabbitMqConnectionString);
    foreach (var job in this.Jobs)
    {
      if (job.StartProcess == null || job.StartProcess < DateTime.Now)
      {
        var message = new GenerateTeamReportRequest
        {
          DaysAgo = 0,
          Team = job.Team,
        };
        await rabbitMqClient.SendMessage(message);
        job.StartProcess = this.Calendar.AddTimeSpanWithHolidays(DateTime.Now, job.RepeatInterval);
        this.Repository.AddOrUpdate(job);
        this.Logger.LogInformation($"Push message for start job for team {job.Team.Name}");
      }
    }
  }
  
  #endregion

  #region Консутркторы
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="repository"></param>
  /// <param name="configuration"></param>
  /// <param name="logger"></param>
  /// <param name="calendar"></param>
  public JobScheduler(IRepository repository, IConfiguration configuration,
    ILogger<JobScheduler> logger, CalendarCalculator calendar)
  {
    this.Repository = repository;
    this.Configuration = configuration;
    this.Logger = logger;
    this.Calendar = calendar;
    this.Jobs = this.Repository.Get<Job>(j => true);
  }
  #endregion
}