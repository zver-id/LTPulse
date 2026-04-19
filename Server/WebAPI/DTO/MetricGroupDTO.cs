namespace WebAPI.DTO;

/// <summary>
/// DTO групп метрик.
/// </summary>
public class MetricGroupDTO
{
  /// <summary>
  /// Id группы.
  /// </summary>
  public int Id { get; set; }
  
  /// <summary>
  /// Имя группы.
  /// </summary>
  public string Name { get; set; }
  
  /// <summary>
  /// Имя графика.
  /// </summary>
  public string NameOfChart {get; set;}
  
  /// <summary>
  /// Тип графика группы.
  /// </summary>
  public string ChartType { get; set; }
}