namespace Application;

/// <summary>
/// Статистика по сотруднику за период.
/// </summary>
public class EmployeeStats
{
  /// <summary>
  /// Имя сотрудника.
  /// </summary>
  public string Employee { get; set; } = string.Empty;

  /// <summary>
  /// Количество назначенных обращений за период.
  /// </summary>
  public int Assigned { get; set; }

  /// <summary>
  /// Количество закрытых обращений за период.
  /// </summary>
  public int Closed { get; set; }

  /// <summary>
  /// Количество обращений в бэклоге (не закрытых) за период.
  /// </summary>
  public int Backlog { get; set; }

  /// <summary>
  /// Среднее время решения в часах.
  /// </summary>
  public float AvgResolutionTime { get; set; }

  /// <summary>
  /// Процент обращений решённых в пределах SLA. Null, если данных нет.
  /// </summary>
  public float? SlaResolution { get; set; }

  /// <summary>
  /// Процент SLA по реакции. Null, если данных нет.
  /// </summary>
  public float? SlaReaction { get; set; }

  /// <summary>
  /// Процент количества ответов. Null, если данных нет.
  /// </summary>
  public float? ResponseCount { get; set; }

  /// <summary>
  /// Процент обращений с эскалацией на линию. Null, если данных нет.
  /// </summary>
  public float? EscalationLine { get; set; }

  /// <summary>
  /// Процент обращений с эскалацией на разработчиков. Null, если данных нет.
  /// </summary>
  public float? EscalationDevs { get; set; }

  /// <summary>
  /// Количество обращений в критической зоне (время в работе &gt;= 24 ч).
  /// </summary>
  public int CriticalZones { get; set; }

  /// <summary>
  /// Количество обращений в оранжевой зоне (16 &lt;= время в работе &lt; 24 ч).
  /// </summary>
  public int OrangeZones { get; set; }

  /// <summary>
  /// Количество обращений в жёлтой зоне (8 &lt;= время в работе &lt; 16 ч).
  /// </summary>
  public int YellowZones { get; set; }

  /// <summary>
  /// Количество обращений в зелёной зоне (время в работе &lt; 8 ч).
  /// </summary>
  public int GreenZones { get; set; }

  /// <summary>
  /// Количество обращений возрастом более 2 недель.
  /// </summary>
  public int Older2Weeks { get; set; }

  /// <summary>
  /// Количество обращений возрастом более 3 недель.
  /// </summary>
  public int Older3Weeks { get; set; }

  /// <summary>
  /// Количество обращений возрастом более 1 месяца (28 дней).
  /// </summary>
  public int Older1Month { get; set; }

  /// <summary>
  /// Процент оценок с максимальной оценкой. Null, если оценок нет.
  /// </summary>
  public float? GradeScore { get; set; }
}
