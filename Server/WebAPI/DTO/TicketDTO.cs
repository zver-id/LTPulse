using System;
using System.ComponentModel.DataAnnotations;
using CommonModels.Interfaces;

namespace WebAPI.DTO;

public class TicketDTO : IDto
{
  /// <summary>
  /// ID тикета.
  /// </summary>
  [Display(Name = "Номер")]
  public int Key { get; set; }
  
  /// <summary>
  /// Наименование.
  /// </summary>
  [Display(Name = "Описание")]
  public string Name { get; set; }
  
  /// <summary>
  /// Тип обращения.
  /// </summary>
  [Display(Name = "Тип обращения")]
  public string Type { get; set; }
  
  /// <summary>
  /// Организация.
  /// </summary>
  [Display(Name = "Организация")]
  public string Organization { get; set; }
  
  /// <summary>
  /// Ответственный сотрудник.
  /// </summary>
  [Display(Name = "Ответственный")]
  public string Employee { get; set; }
  
  /// <summary>
  /// Приоритет.
  /// </summary>
  [Display(Name = "Приоритет")]
  public virtual string Priority { get; set; }
  
  /// <summary>
  /// Дата поступления.
  /// </summary>
  [Display(Name = "Дата поступления")]
  public virtual DateTime IncomingDate { get; set; }
  
  /// <summary>
  /// Состояние.
  /// </summary>
  [Display(Name = "Состояние")]
  public string State { get; set; }
  
  /// <summary>
  /// Время в работе.
  /// </summary>
  [Display(Name = "Время в работе")]
  public float TimeInWork { get; set; }
  
  /// <summary>
  /// Время отмеченное за день.
  /// </summary>
  [Display(Name = "Время отмеченное за день")]
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