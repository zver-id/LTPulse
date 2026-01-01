using System.Diagnostics;
using CommonModels.Models;
using DBCore;
using TechKasConnector.MetricCreators;

namespace TechKasConnector.Tests;

public class Tests
{
  private GenericMetricCreator metricCreator;
  private DBRepository repository;
  [SetUp]
  public void Setup()
  {
    this.repository = new DBRepository();
    var team = this.repository.Get<Team>(x => x.Name == "ОГВ").FirstOrDefault();
    this.metricCreator = new GenericMetricCreator(this.repository, team);
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

  [TearDown]
  public void TearDown()
  {
    this.repository.Dispose();
  }
}