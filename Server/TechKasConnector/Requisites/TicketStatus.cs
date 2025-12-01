namespace TechKasConnector.Requisites;

/// <summary>
/// Статусы обращений.
/// </summary>
public static class TicketStatus
{
  public static readonly string Initialization = "И";
  public static readonly string InWork = "Р";
  public static readonly string OnControl = "К";
  public static readonly string Forwarded = "П";
  public static readonly string Closed = "З";

  public static readonly List<string> Active = [Initialization, InWork, OnControl, Forwarded];


}