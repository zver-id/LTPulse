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
  private RabbitMQClient RabbitMQClient { get; }
  
  /// <summary>
  /// Список задач.
  /// </summary>
  private List<Job> Jobs { get; set; }

  #endregion

  #region Методы

  /// <summary>
  /// Создает задачи для новых команд.
  /// </summary>
  public async Task CreateJobsForNewTeams()
  {
    this.Logger.LogInformation("Create jobs for new teams.");
    var teams = await this.Repository.GetAsync<Team>(t => true);
    if (teams.Count == 0)
      throw new InvalidOperationException("В базе данных нет команд.");
    this.Jobs = await this.Repository.GetAsync<Job>(j => true);
    foreach (var team in teams)
    {
      if (this.Jobs.All(j => j.Team.Id != team.Id))
      {
        var newJob = new Job
        {
          Name = $"generate_report_{team.Name}",
          Team = team,
          StartProcess = DateTime.Now,
          RepeatInterval = TimeSpan.FromMinutes(180)
        };
        await this.Repository.AddOrUpdate(newJob);
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
    if (this.Jobs == null)
      await this.CreateJobsForNewTeams();
    foreach (var job in this.Jobs)
    {
      if ((job.StartProcess == null || job.StartProcess < DateTime.Now) && !job.InProgress)
      {
        var message = new GenerateTeamReportRequest
        {
          DaysAgo = 0,
          TeamId = job.Team.Id,
        };
        await this.RabbitMQClient.SendMessage(message);
        job.InProgress = true;
        //job.StartProcess = await this.Calendar.AddTimeSpanWithHolidays(DateTime.Now, job.RepeatInterval);
        await this.Repository.AddOrUpdate(job);
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
  /// <param name="rabbitMQClient"></param>
  public JobScheduler(IRepository repository, IConfiguration configuration,
    ILogger<JobScheduler> logger, CalendarCalculator calendar, RabbitMQClient rabbitMQClient)
  {
    this.Repository = repository;
    this.Configuration = configuration;
    this.Logger = logger;
    this.Calendar = calendar;
    this.RabbitMQClient = rabbitMQClient;
  }
  #endregion
}