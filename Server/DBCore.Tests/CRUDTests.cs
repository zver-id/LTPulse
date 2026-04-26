using System.Globalization;
using CommonModels.Models;
using NHibernate.Infrastructure;

namespace DBCore.Tests;

public class CRUDTests
{
  private DbRepository repository;
  
  [SetUp]
  public void Setup()
  {
    repository = new DbRepository(new NhibernateHelper("Host=localhost;Port=5432;Database=LTPulse;Username=admin;Password=Qwerty123"));
  }

  //[Test]
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

  //[Test]
  public void AddTicket()
  {
    var ticket = new Ticket
    {
      Id = 101,
      Name = "Имя 22тестовое",
      Organization = "Имя организации",
      Employee = "Имя сотрудника",
      Priority = this.repository.GetAsync<Priority>(x => x.Name == "Низкий").First(),
      IncomingDate = DateTime.Now,
      State = this.repository.GetAsync<TicketState>(s =>
        s.State == "В работе").First(),
      TimeInWork = 0,
      Hyperlink = "нет ничо"
    };

    repository.AddOrUpdate(ticket);
    
    Assert.DoesNotThrow(
      () => repository.AddOrUpdate(ticket));
  }
  
  //[Test]
  public void AddUpdateExistingMetric()
  {
    var metric = new Metric
    {
      Date = DateTime.Today,
      MetricType = this.repository.GetAsync<MetricType>(x => x.Name == "Всего в работе").First(),
      Team = this.repository.GetAsync<Team>(x => x.Name == "ОГВ").First(),
      Value = 1
    };

    this.repository.AddOrUpdate(metric);
    
    var metric2 = new Metric
    {
      Date = DateTime.Today,
      MetricType = this.repository.GetAsync<MetricType>(x => x.Name == "Всего в работе").First(),
      Team = this.repository.GetAsync<Team>(x => x.Name == "ОГВ").First(),
      Value = 4
    };
    
    Assert.DoesNotThrow(
      () => repository.AddOrUpdate(metric2));
  }

  [Test]
  public void AddTicketsToMetricAndSaveList()
  {
    var ticket = new Ticket
    {
      Id = 101,
      Name = "Имя 22тестовое",
      Organization = "Имя организации",
      Employee = "Имя сотрудника",
      Priority = this.repository.GetAsync<Priority>(x => x.Name == "Низкий").First(),
      IncomingDate = DateTime.Now,
      State = this.repository.GetAsync<TicketState>(s =>
        s.State == "В работе").First(),
      TimeInWork = 0,
      Hyperlink = "нет ничо"
    };
    
    var metric = new Metric
    {
      Date = DateTime.Today,
      MetricType = this.repository.GetAsync<MetricType>(x => x.Name == "Всего в работе").First(),
      Team = this.repository.GetAsync<Team>(x => x.Name == "ОГВ").First(),
      Value = 1
    };
    
    metric.Tickets.Add(ticket);
    this.repository.AddOrUpdate(ticket);
    this.repository.AddOrUpdate(metric);
    
    var existMetric = this.repository.GetAsync<Metric>(x => x.Date == DateTime.Today).First();
    Assert.AreEqual(existMetric.Tickets.Count, 1);
  }

  [TearDown]
  public void TearDown()
  {
    repository.Dispose();
  }
}