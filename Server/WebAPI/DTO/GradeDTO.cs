namespace WebAPI.DTO;

public class GradeDTO
{
  /// <summary>
  /// Номер обращения по оценке.
  /// </summary>
  public int Key { get; set; }
  
  /// <summary>
  /// Текст оценки.
  /// </summary>
  public string Text { get; set; }
  
  /// <summary>
  /// Балл оценки.
  /// </summary>
  public int Score { get; set; }
  
  /// <summary>
  /// Дата оценки.
  /// </summary>
  public string Date { get; set; }
  
  /// <summary>
  /// Оценка проработана.
  /// </summary>
  public bool isResearched { get; set; }
  
  /// <summary>
  /// Ссылка на обращение.
  /// </summary>
  public string TicketHyperlink { get; set; }
  
  /// <summary>
  /// Сотрудник.
  /// </summary>
  public string Employee { get; set; }
}