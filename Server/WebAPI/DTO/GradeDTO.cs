using System.ComponentModel.DataAnnotations;
using CommonModels.Interfaces;

namespace WebAPI.DTO;

public class GradeDTO : IDto
{
  /// <summary>
  /// Номер обращения по оценке.
  /// </summary>
  [Display(Name = "Номер обращения")]
  public int Key { get; set; }
  
  /// <summary>
  /// Текст оценки.
  /// </summary>
  [Display(Name = "Текст оценки")]
  public string Text { get; set; }
  
  /// <summary>
  /// Балл оценки.
  /// </summary>
  [Display(Name = "Балл оценки")]
  public int Score { get; set; }
  
  /// <summary>
  /// Дата оценки.
  /// </summary>
  [Display(Name = "Дата оценки")]
  public string Date { get; set; }
  
  /// <summary>
  /// Оценка проработана.
  /// </summary>
  [Display(Name = "Проработано")]
  public bool isResearched { get; set; }
  
  /// <summary>
  /// Ссылка на обращение.
  /// </summary>
  public string Hyperlink { get; set; }
  
  /// <summary>
  /// Сотрудник.
  /// </summary>
  [Display(Name = "Сотрудник")]
  public string Employee { get; set; }
}