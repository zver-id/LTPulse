namespace CommonModels.Interfaces;

/// <summary>
/// Объект, имеющий ID.
/// </summary>
public interface IHasId
{
  /// <summary>
  /// Id объекта.
  /// </summary>
  public int Id { get; set; }
}