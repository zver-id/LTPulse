using CommonModels.Models;

namespace DBCore.Tests;

public class CRUDTests
{
  private DBReposytory repository;
  
  [SetUp]
  public void Setup()
  {
    repository = new DBReposytory();
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