using System.Text.Json.Serialization;

namespace TechKasConnector.Mattermost;

/// <summary>
/// Сообщение Mattermost.
/// </summary>
public class MattermostPost
{
  /// <summary>
  /// ИД сообщения.
  /// </summary>
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// ИД канала.
  /// </summary>
  [JsonPropertyName("channel_id")]
  public string ChannelId { get; set; } = string.Empty;

  /// <summary>
  /// ИД команды.
  /// </summary>
  [JsonPropertyName("team_id")]
  public string TeamId { get; set; } = string.Empty;

  /// <summary>
  /// ИД автора.
  /// </summary>
  [JsonPropertyName("user_id")]
  public string UserId { get; set; } = string.Empty;

  /// <summary>
  /// Текст сообщения.
  /// </summary>
  [JsonPropertyName("message")]
  public string Message { get; set; } = string.Empty;

  /// <summary>
  /// Время создания (миллисекунды от эпохи).
  /// </summary>
  [JsonPropertyName("create_at")]
  public long CreateAt { get; set; }

  /// <summary>
  /// Время в DateTime.
  /// </summary>
  [JsonIgnore]
  public DateTime Created
  {
    get
    {
      return DateTimeOffset.FromUnixTimeMilliseconds(this.CreateAt).LocalDateTime;
    }
  }

  /// <summary>
  /// Реплики к сообщению.
  /// </summary>
  [JsonPropertyName("props")]
  public Dictionary<string, object> Props { get; set; } = new();
}
