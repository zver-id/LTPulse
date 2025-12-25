using System.Globalization;
using CommonModels.Models;

namespace DBCore.Tests;

public class CRUDTests
{
  private DBRepository repository;
  
  [SetUp]
  public void Setup()
  {
    repository = new DBRepository();
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
    var expectedTeam = repository.Get<Team>("Name", team.Name);
    
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
      Priority = this.repository.GetByPredicate<Priority>(x => x.Name == "Низкий").First(),
      IncomingDate = DateTime.Now,
      State = this.repository.GetByPredicate<TicketState>(s =>
        s.State == "В работе").First(),
      TimeInWork = 0,
      Hyperlink = "нет ничо"
    };

    repository.AddOrUpdate(ticket);
    
    Assert.DoesNotThrow(
      () => repository.AddOrUpdate(ticket));
  }

  [TearDown]
  public void TearDown()
  {
    repository.Dispose();
  }
}