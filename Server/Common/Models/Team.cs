using System.Collections.Generic;
using Common.Interfaces;

namespace Common.Models;

/// <summary>
/// Команда.
/// </summary>
public class Team : IHasId
{
  /// <summary>
  /// ID команды.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Имя команды.
  /// </summary>
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Участники команды.
  /// </summary>
  public virtual IList<Employee> Employees { get; set; }
}