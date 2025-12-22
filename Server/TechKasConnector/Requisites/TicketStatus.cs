namespace TechKasConnector.Requisites;

/// <summary>
/// Статусы обращений.
/// </summary>
public static class TicketStatus
{
  /// <summary>
  /// На инициализации.
  /// </summary>
  public static readonly string Initialization = "И";
  
  /// <summary>
  /// В работе.
  /// </summary>
  public static readonly string InWork = "Р";
  
  /// <summary>
  /// На контроле.
  /// </summary>
  public static readonly string OnControl = "К";
  
  /// <summary>
  /// Переадресовано.
  /// </summary>
  public static readonly string Forwarded = "П";
  
  /// <summary>
  /// Закрыто.
  /// </summary>
  public static readonly string Closed = "З";

  /// <summary>
  /// Незакрытие обращения (в работе, на контроле и т.д.)
  /// </summary>
  public static readonly List<string> Active = [Initialization, InWork, OnControl, Forwarded];


}