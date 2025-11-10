using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;

namespace XLStoDBConverter;

public class BaseTypeInitializer
{
  private DBRepository dbRepository { get; init; }
  
  public void TryAddTypes(List<IHasId> types)
  {
    foreach (var item in types)
    {
      try
      {
        this.dbRepository.Add(item);
        Console.WriteLine($"Объект {nameof(item)} с {item.Id} добавлен");
      }
      catch (NHibernate.Exceptions.GenericADOException ex)
      {
        Console.WriteLine($"Объект {nameof(item)} с {item.Id} пропущен");
      }
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
      new Team {Name = "Аврора"},
      new Team {Name = "Атлас"},
      new Team {Name = "Бета"},
      new Team {Name = "ОГВ"},
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