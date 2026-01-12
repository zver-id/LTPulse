using System;
using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Обращение.
/// </summary>
public class Ticket : IHasId
{
  /// <summary>
  /// ID.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Наименование.
  /// </summary>
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Тип обращения.
  /// </summary>
  public virtual string Type { get; set; }
  
  /// <summary>
  /// Организация.
  /// </summary>
  public virtual string Organization { get; set; }
  
  /// <summary>
  /// Ответственный сотрудник.
  /// </summary>
  public virtual string Employee { get; set; }
  
  /// <summary>
  /// Приоритет.
  /// </summary>
  public virtual Priority Priority { get; set; }
  
  /// <summary>
  /// Дата поступления.
  /// </summary>
  public virtual DateTime IncomingDate { get; set; }
  
  /// <summary>
  /// Состояние.
  /// </summary>
  public virtual TicketState State { get; set; }
  
  /// <summary>
  /// Время в работе.
  /// </summary>
  public virtual float TimeInWork { get; set; }
  
  /// <summary>
  /// Время отмеченное за день.
  /// </summary>
  public virtual float TimeStampedOnDay { get; set; }
  
  /// <summary>
  /// Гиперссылка.
  /// </summary>
  public virtual string Hyperlink { get; set; }
}