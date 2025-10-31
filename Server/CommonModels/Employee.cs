namespace CommonModels;

/// <summary>
/// Сотрудник.
/// </summary>
public class Employee
{
  /// <summary>
  /// ID.
  /// </summary>
  public int Id { get; set; }
  
  /// <summary>
  /// Имя и фамилия.
  /// </summary>
  public string Name { get; set; }
  
  /// <summary>
  /// Табельный номер.
  /// </summary>
  public int PersonnelNumber { get; set; }
  
  /// <summary>
  /// Номер карточки в ТехКАС.
  /// </summary>
  public int TechKASNumber { get; set; }
  
  /// <summary>
  /// Команда.
  /// </summary>
  public Team Team { get; set; }
}