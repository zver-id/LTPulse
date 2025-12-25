using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Жизненная стадия обращения.
/// </summary>
public class TicketState : IHasId
{
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Название стадии.
  /// </summary>
  public virtual string State { get; set; }
}