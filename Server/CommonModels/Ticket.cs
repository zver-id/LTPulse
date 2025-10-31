using System;

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
  /// Организация.
  /// </summary>
  public string Organization { get; set; }
  
  /// <summary>
  /// Ответственный сотрудник.
  /// </summary>
  public string Employee { get; set; }
  
  /// <summary>
  /// Приоритет.
  /// </summary>
  public Priority Priority { get; set; }
  
  /// <summary>
  /// Дата поступления.
  /// </summary>
  public DateOnly IncomingDate { get; set; }
  
  /// <summary>
  /// Состояние.
  /// </summary>
  public TicketState State { get; set; }
  
  /// <summary>
  /// Время в работе.
  /// </summary>
  public float TimeInWork { get; set; }
}