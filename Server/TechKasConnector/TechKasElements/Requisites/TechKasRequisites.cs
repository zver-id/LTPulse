namespace TechKasConnector.Requisites;

/// <summary>
/// Реквизиты справочников в ТехКас.
/// </summary>
public static class TechKasRequisites
{
  public const string Id = "Код";
  public const string Name = "Содержание";
  public const string Organization = "Организация";
  public const string Employee = "Работник";
  public const string TicketType = "ТипОбращения";
  public const string OpenDate = "ДатОткр";
  public const string ClosedDate = "ДатЗакр";
  public const string TicketStatus = "СостОбращения";
  public const string SupportArea = "ОбластьПоддержки";
  public const string Priority = "Строка3";

  public const string EmployeeDetail = "РаботникТ2";
  public const string DateDetail = "ДатаТ2";
  public const string TimeSpent = "Сумма5Т2";
  
  public const string TicketStatusDetail = "СостОбращенияТ4";
  
  /// <summary>
  /// Дата и время изменения статуса обращения в дополнительной таблице.
  /// </summary>
  public const string DateStatusDetail = "ДатаВремяT4";
  
  /// <summary>
  /// Номер обращения, которое соответствует оценке.
  /// </summary>
  public const string GradeTicketNum = "Обращение";
  
  /// <summary>
  /// Оценка по обращению.
  /// </summary>
  public const string GradeScore = "ISBIntNumber";

  /// <summary>
  /// Текст оценки.
  /// </summary>
  public const string GradeText = "Текст";
  
  /// <summary>
  /// Текст оценки.
  /// </summary>
  public const string GradeDate = "ДатаВремя";
}