using Common.Models;

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
}