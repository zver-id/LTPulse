using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Канал Mattermost.
/// </summary>
public class MattermostChannel : IHasId
{
  public virtual int Id { get; set; }

  /// <summary>
  /// Наименование канала.
  /// </summary>
  public virtual string Name { get; set; }

  /// <summary>
  /// Идентификатор канала в Mattermost.
  /// </summary>
  public virtual string ChannelId { get; set; }

  /// <summary>
  /// Тип канала: Line — линия поддержки, Dev — разработчики.
  /// </summary>
  public virtual ChannelType Type { get; set; }
}

public enum ChannelType
{
  Line,
  Dev
}
