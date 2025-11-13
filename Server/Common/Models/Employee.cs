using Common.Interfaces;

namespace Common.Models;

/// <summary>
/// Сотрудник.
/// </summary>
public class Employee : IHasId
{
  /// <summary>
  /// ID.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Имя и фамилия.
  /// </summary>
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Табельный номер.
  /// </summary>
  public virtual int PersonnelNumber { get; set; }
  
  /// <summary>
  /// Номер карточки в ТехКАС.
  /// </summary>
  public virtual int TechKASNumber { get; set; }
  
  /// <summary>
  /// Команда.
  /// </summary>
  public virtual Team Team { get; set; }
}