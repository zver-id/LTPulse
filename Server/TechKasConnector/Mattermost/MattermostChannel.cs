using System.Text.Json.Serialization;

namespace TechKasConnector.Mattermost;

/// <summary>
/// Канал Mattermost.
/// </summary>
public class MattermostChannel
{
  /// <summary>
  /// ИД канала.
  /// </summary>
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// ИД команды.
  /// </summary>
  [JsonPropertyName("team_id")]
  public string TeamId { get; set; } = string.Empty;

  /// <summary>
  /// Название канала.
  /// </summary>
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// DisplayName канала.
  /// </summary>
  [JsonPropertyName("display_name")]
  public string DisplayName { get; set; } = string.Empty;
}
