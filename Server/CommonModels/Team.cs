using System.Collections.Generic;

namespace CommonModels;

/// <summary>
/// Команда.
/// </summary>
public class Team
{
  /// <summary>
  /// ID команды.
  /// </summary>
  public int Id { get; set; }
  
  /// <summary>
  /// Имя команды.
  /// </summary>
  public string Name { get; set; }
  
  /// <summary>
  /// Участники команды.
  /// </summary>
  public List<Employee> Employees { get; set; }
}