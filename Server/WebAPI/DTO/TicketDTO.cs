using System;

namespace WebAPI.DTO;

public class TicketDTO
{
  /// <summary>
  /// ID тикета.
  /// </summary>
  public int Key { get; set; }
  
  /// <summary>
  /// Наименование.
  /// </summary>
  public string Name { get; set; }
  
  /// <summary>
  /// Тип обращения.
  /// </summary>
  public string Type { get; set; }
  
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
  public virtual string Priority { get; set; }
  
  /// <summary>
  /// Дата поступления.
  /// </summary>
  public virtual DateTime IncomingDate { get; set; }
  
  /// <summary>
  /// Состояние.
  /// </summary>
  public string State { get; set; }
  
  /// <summary>
  /// Время в работе.
  /// </summary>
  public float TimeInWork { get; set; }
  
  /// <summary>
  /// Время отмеченное за день.
  /// </summary>
  public float TimeStampedOnDay { get; set; }
  
  /// <summary>
  /// Гиперссылка.
  /// </summary>
  public string Hyperlink { get; set; }
  
  /// <summary>
  /// Комментарий.
  /// </summary>
  public string Comment { get; set; }
}