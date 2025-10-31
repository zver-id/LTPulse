using System.Security;

namespace CommonModels;

/// <summary>
/// Оценка обращения.
/// </summary>
public class Grade
{
  /// <summary>
  /// ID.
  /// </summary>
  public int Id { get; set; }
  
  /// <summary>
  /// Текст оценки.
  /// </summary>
  public string Text { get; set; }
  
  /// <summary>
  /// Результат.
  /// </summary>
  public int Score { get; set; }
  
  /// <summary>
  /// Обращение по которому поступила оценка.
  /// </summary>
  public Ticket Ticket { get; set; }
}