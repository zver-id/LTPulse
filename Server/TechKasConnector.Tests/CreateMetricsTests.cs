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

  [TearDown]
  public void TearDown()
  {
    this.repository.Dispose();
  }
}