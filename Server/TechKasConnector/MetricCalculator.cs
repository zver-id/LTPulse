using CommonModels.Models;
using DBCore;

namespace TechKasConnector;

public class MetricCalculator
{
  #region Поля и свойства
  
  /// <summary>
  /// Справочник обращений.
  /// </summary>
  private TechKasReference tickets;
  
  /// <summary>
  /// Репозиторий.
  /// </summary>
  private DBRepository repository;

  /// <summary>
  /// Команда, для которой идет расчет.
  /// </summary>
  private Team team;
  
  #endregion
  
  #region Методы
  
  #endregion
  
  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  public MetricCalculator(Team team)
  {
    this.team = team;
    this.repository = new DBRepository();
    this.tickets = new TechKasReference("ПДД");
    List<TechKasFilter> initFilters = this.repository.GetByPredicate<TechKasFilter>(
      f=> f.Team == null || f.Team.Id == this.team.Id)
      .ToList();
    Dictionary<string, List<string>> includeFilters = initFilters
      .Where(f => f.ShouldInclude == true)
      .GroupBy(f => f.NameOfField)
      .ToDictionary(
        f => f.Key,
        f => f.Select(f=> f.Value).ToList()
        );
    
    foreach (var filter in initFilters)
    {
      
    }

  }
  
  #endregion
}