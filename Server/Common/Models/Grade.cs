using Common.Interfaces;

namespace Common.Models;

/// <summary>
/// Оценка обращения.
/// </summary>
public class Grade : IHasId
{
  /// <summary>
  /// ID.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Текст оценки.
  /// </summary>
  public virtual string Text { get; set; }
  
  /// <summary>
  /// Результат.
  /// </summary>
  public virtual int Score { get; set; }
  
  /// <summary>
  /// Обращение по которому поступила оценка.
  /// </summary>
  public virtual Ticket Ticket { get; set; }
}