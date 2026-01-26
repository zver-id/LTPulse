using System.Diagnostics;
using Application;
using Application.RabbitMQRequests;
using CommonModels.Models;
using DBCore;
using TechKasConnector.MetricCreators;

namespace TechKasConnector.Tests;

public class Tests
{
  private GenericMetricCreator metricCreator;
  private DbRepository repository;
  private Team team;
  [SetUp]
  public void Setup()
  {
    this.repository = new DbRepository();
    this.team = this.repository.Get<Team>(x => x.Name == "ОГВ").FirstOrDefault();
    //this.metricCreator = new GenericMetricCreator(this.repository, team);
  }

  [Test]
  public void CreateMonthMetrics()
  {
    this.metricCreator.CreateMonthMetrics();
    var metrics = this.repository.Get<Metric>(x => x.Date == DateTime.Today).FirstOrDefault();
    Assert.NotNull(metrics);
  }

  [Test]
  public void GetAllTicketsLessThatTenSeconds()
  {
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    var result = this.metricCreator.GetAllTicketsWithTime();
    stopwatch.Stop();
    var elapsed = stopwatch.ElapsedMilliseconds;
    Console.WriteLine("Get all tickets with time: " + elapsed);
    Assert.Less(elapsed, 60000);
  }
  
  [Test]
  public void CreateMonthsMetric()
  {
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    this.metricCreator.CreateMonthMetrics();
    stopwatch.Stop();
    var elapsed = stopwatch.ElapsedMilliseconds;
    Console.WriteLine("Create months metric: " + elapsed);
    Assert.Less(elapsed, 60000);
  }

  [Test]
  public async Task SendRabbitMQMessage()
  {
    var rabbitProduser = await RabbitMQClient.CreateAsync("amqp://admin:Qwerty123@localhost:5672/stathost");
    var generateReportMessage = new GenerateTeamReportRequest
    {
      Team = this.team,
      DaysAgo = 1
    };
    rabbitProduser.SendMessage(generateReportMessage);
  }

  [TearDown]
  public void TearDown()
  {
    this.repository.Dispose();
  }
}