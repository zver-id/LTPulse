using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Приоритет обращения.
/// </summary>
public class Priority : IHasId
{
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Название приоритета.
  /// </summary>
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Время на решение.
  /// </summary>
  public virtual int TimeToSolve { get; set; }
}