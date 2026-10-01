using System.Text.Json.Serialization;

namespace TechKasConnector.Mattermost;

/// <summary>
/// Результат поиска сообщений в Mattermost.
/// </summary>
public class MattermostSearchResult
{
  /// <summary>
  /// Найденные сообщения.
  /// </summary>
  [JsonPropertyName("posts")]
  public Dictionary<string, MattermostPost> Posts { get; set; } = new();

  /// <summary>
  /// ИД сообщений, отсортированных по дате.
  /// </summary>
  [JsonPropertyName("order")]
  public List<string> Order { get; set; } = new();

  /// <summary>
  /// Общее количество найденных.
  /// </summary>
  [JsonPropertyName("total")]
  public int Total { get; set; }

  /// <summary>
  /// Упорядоченные сообщения.
  /// </summary>
  [JsonIgnore]
  public IReadOnlyList<MattermostPost> OrderedPosts
  {
    get
    {
      return this.Order
        .Where(id => this.Posts.TryGetValue(id, out var post))
        .Select(id => this.Posts[id])
        .ToList();
    }
  }
}
