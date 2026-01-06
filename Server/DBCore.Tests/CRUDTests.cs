using System.Globalization;
using CommonModels.Models;

namespace DBCore.Tests;

public class CRUDTests
{
  private DbRepository repository;
  
  [SetUp]
  public void Setup()
  {
    repository = new DbRepository();
  }

  [Test]
  public void Add_AddTeam_AddedSuccessfully()
  {
    var team = new Team
    {
      Id = 1,
      Name = "Аврора"
    };
    
    repository.Add(team);
    var expectedTeam = repository.GetByField<Team>("Name", team.Name);
    
    Assert.AreEqual(expectedTeam.Name, team.Name);
  }

  [Test]
  public void AddTicket()
  {
    var ticket = new Ticket
    {
      Id = 101,
      Name = "Имя 22тестовое",
      Organization = "Имя организации",
      Employee = "Имя сотрудника",
      Priority = this.repository.Get<Priority>(x => x.Name == "Низкий").First(),
      IncomingDate = DateTime.Now,
      State = this.repository.Get<TicketState>(s =>
        s.State == "В работе").First(),
      TimeInWork = 0,
      Hyperlink = "нет ничо"
    };

    repository.AddOrUpdate(ticket);
    
    Assert.DoesNotThrow(
      () => repository.AddOrUpdate(ticket));
  }
  
  [Test]
  public void AddUpdateExistingMetric()
  {
    var metric = new Metric
    {
      Date = DateTime.Today,
      MetricType = this.repository.Get<MetricType>(x => x.Name == "Всего в работе").First(),
      Team = this.repository.Get<Team>(x => x.Name == "ОГВ").First(),
      Value = 1
    };

    this.repository.AddOrUpdate(metric);
    
    var metric2 = new Metric
    {
      Date = DateTime.Today,
      MetricType = this.repository.Get<MetricType>(x => x.Name == "Всего в работе").First(),
      Team = this.repository.Get<Team>(x => x.Name == "ОГВ").First(),
      Value = 4
    };
    
    Assert.DoesNotThrow(
      () => repository.AddOrUpdate(metric2));
  }

  [TearDown]
  public void TearDown()
  {
    repository.Dispose();
  }
}