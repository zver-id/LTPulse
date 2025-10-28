namespace CommonModels;

/// <summary>
/// Обращение.
/// </summary>
public class Ticket
{
  /// <summary>
  /// ID.
  /// </summary>
  public int Id { get; set; }
  
  /// <summary>
  /// Наименование.
  /// </summary>
  public string Name { get; set; }
  
  /// <summary>
  /// Приоритет.
  /// </summary>
  public Priority Priority { get; set; }
}