namespace TechKasConnector.Requisites;

/// <summary>
/// Типы обращений.
/// </summary>
public static class TicketType
{
  public static readonly string Incident = "И";
  public static readonly string Consultation = "К";
  public static readonly string Request = "З";
  public static readonly string Problem = "П";

  public static readonly List<string> WithoutProblems = [Incident, Consultation, Request];
  public static readonly List<string> IncidentsConsultation = [Incident, Consultation];
}