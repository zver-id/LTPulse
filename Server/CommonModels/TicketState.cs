namespace CommonModels;

/// <summary>
/// Жизненная стадия обращения.
/// </summary>
public enum TicketState
{
  Initialization,
  AtWork,
  UnderControl,
  Forwarded,
  AtClosing,
  Closed
}