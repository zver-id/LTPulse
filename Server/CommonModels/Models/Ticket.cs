using System;
using CommonModels.Interfaces;
using CommonModels.Attributes;

namespace CommonModels.Models;

/// <summary>
/// Обращение.
/// </summary>
public class Ticket : IHasId
{
  /// <summary>
  /// ID.
  /// </summary>
  [DisplayName("Номер обращения")]
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Наименование.
  /// </summary>
  [DisplayName("Описание")]
  public virtual string Name { get; set; }
  
  /// <summary>
  /// Тип обращения.
  /// </summary>
  [DisplayName("Тип обращения")]
  public virtual string Type { get; set; }
  
  /// <summary>
  /// Организация.
  /// </summary>
  [DisplayName("Организация")]
  public virtual string Organization { get; set; }
  
  /// <summary>
  /// Ответственный сотрудник.
  /// </summary>
  [DisplayName("Ответственный")]
  public virtual string Employee { get; set; }
  
  /// <summary>
  /// Приоритет.
  /// </summary>
  [DisplayName("Приоритет")]
  public virtual Priority Priority { get; set; }
  
  /// <summary>
  /// Дата поступления.
  /// </summary>
  [DisplayName("Дата поступления")]
  public virtual DateTime IncomingDate { get; set; }
  
  /// <summary>
  /// Состояние.
  /// </summary>
  [DisplayName("Состояние")]
  public virtual TicketState State { get; set; }
  
  /// <summary>
  /// Время в работе.
  /// </summary>
  [DisplayName("Время в работе")]
  public virtual float TimeInWork { get; set; }
  
  /// <summary>
  /// Время отмеченное за день.
  /// </summary>
  [DisplayName("Отметка за день")]
  public virtual float TimeStampedOnDay { get; set; }
  
  /// <summary>
  /// Гиперссылка.
  /// </summary>
  public virtual string Hyperlink { get; set; }
}