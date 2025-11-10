using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;

namespace XLStoDBConverter;

public class BaseTypeInitializer
{
  private DBRepository dbRepository { get; init; }
  
  public void TryAddTypes(List<IHasId> types)
  {
    try
    {
      foreach (var item in types)
        this.dbRepository.Add(item);
      Console.WriteLine("Success");
    }
    catch (NHibernate.Exceptions.GenericADOException ex)
    {
      Console.WriteLine(ex.Message);
      Console.WriteLine(ex.StackTrace);
    }
  }
  
  public void AddMetricTypes()
  {
    List<IHasId> metricTypes =
    [
      new MetricType("OlderThanTwoWeeks"),
      new MetricType("OlderThanThreeWeeks"),
      new MetricType("OlderFourTwoWeeks"),
      new MetricType("TotalInProgress"),
      new MetricType("NotClosed"),
      new MetricType("ExternalMessages"),
      new MetricType("GreenZone"),
      new MetricType("SandyZone"),
      new MetricType("YellowZone"),
      new MetricType("RedZone"),
      new MetricType("NegativeGrades"),
      new MetricType("WorkedNegativeGrades"),
      new MetricType("IncomingIncidents"),
      new MetricType("IncomingConsultations"),
      new MetricType("IncomingRequests"),
      new MetricType("IncomingProblems"),
      new MetricType("IncomingTotal"),
      new MetricType("SpentOnRequests")
    ];

    this.TryAddTypes(metricTypes);
  }

  public void AddTeams()
  {
    List<IHasId> teams =
    [
      new Team {Name = "Aurora"},
      new Team {Name = "Atlas"},
      new Team {Name = "Beta"},
      new Team {Name = "OGV"},
      new Team {Name = "Sokongan"},
      new Team {Name = "Sierra Dogs"},
    ];
    
    this.TryAddTypes(teams);
  }

  public BaseTypeInitializer(DBRepository repository)
  {
    this.dbRepository = repository;
  }
}