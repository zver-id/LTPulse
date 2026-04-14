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
  
  public static readonly string IncidentFull = "Инцидент";
  public static readonly string ConsultationFull = "Консультация";
  public static readonly string RequestFull = "Запрос на обслуживание";
  public static readonly string ProblemFull = "Проблема";

  public static readonly List<string> WithoutProblems = [Incident, Consultation, Request];
  public static readonly List<string> IncidentsConsultation = [Incident, Consultation];
  
  public static readonly List<string> WithoutProblemsFull = [IncidentFull, ConsultationFull, RequestFull];
  public static readonly List<string> IncidentsConsultationFull = [IncidentFull, ConsultationFull];
}