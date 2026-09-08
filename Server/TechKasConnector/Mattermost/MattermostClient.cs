using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TechKasConnector.Mattermost;

/// <summary>
/// Клиент для обращения к API Mattermost.
/// </summary>
public class MattermostClient
{
  private const int PageLimit = 200;

  private readonly HttpClient httpClient;
  private readonly ILogger<MattermostClient> logger;

  #region Методы

  /// <summary>
  /// Найти сообщения в указанном канале по регулярному выражению.
  /// </summary>
  /// <param name="channelId">ИД канала.</param>
  /// <param name="pattern">Регулярное выражение.</param>
  /// <param name="caseSensitive">Учитывать регистр.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Найденные сообщения.</returns>
  public async Task<IReadOnlyList<MattermostPost>> SearchChannelByRegex(
    string channelId, string pattern, bool caseSensitive = false, CancellationToken cancellationToken = default)
  {
    var result = await this.SearchPosts(channelId: channelId, pattern: pattern,
      useRegex: true, caseSensitive: caseSensitive, cancellationToken: cancellationToken);
    return result.OrderedPosts;
  }

  /// <summary>
  /// Найти сообщения по регулярному выражению во всех каналах.
  /// </summary>
  /// <param name="pattern">Регулярное выражение.</param>
  /// <param name="caseSensitive">Учитывать регистр.</param>
  /// <param name="teamId">Ограничить поиском командой (необязательно).</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Найденные сообщения.</returns>
  public async Task<IReadOnlyList<MattermostPost>> SearchByRegex(
    string pattern, bool caseSensitive = false, string? teamId = null, CancellationToken cancellationToken = default)
  {
    var result = await this.SearchPosts(channelId: null, pattern: pattern,
      useRegex: true, caseSensitive: caseSensitive, teamId: teamId, cancellationToken: cancellationToken);
    return result.OrderedPosts;
  }

  /// <summary>
  /// Найти сообщения в указанном канале по строке (без регулярного выражения).
  /// </summary>
  /// <param name="channelId">ИД канала.</param>
  /// <param name="term">Искомая строка.</param>
  /// <param name="caseSensitive">Учитывать регистр.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Найденные сообщения.</returns>
  public async Task<IReadOnlyList<MattermostPost>> SearchChannel(
    string channelId, string term, bool caseSensitive = false, CancellationToken cancellationToken = default)
  {
    var result = await this.SearchPosts(channelId: channelId, pattern: term,
      useRegex: false, caseSensitive: caseSensitive, cancellationToken: cancellationToken);
    return result.OrderedPosts;
  }

  /// <summary>
  /// Получить список каналов команды.
  /// </summary>
  /// <param name="teamId">ИД команды.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <returns>Каналы.</returns>
  public async Task<IReadOnlyList<MattermostChannel>> GetTeamChannels(
    string teamId, CancellationToken cancellationToken = default)
  {
    using var response = await this.HttpGetAsync($"/teams/{teamId}/channels", cancellationToken);
    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    var channels = JsonSerializer.Deserialize<List<MattermostChannel>>(body)
      ?? new List<MattermostChannel>();
    return channels;
  }

  /// <summary>
  /// Выполнить поиск сообщений через /posts/search.
  /// </summary>
  private async Task<MattermostSearchResult> SearchPosts(
    string? channelId, string pattern, bool useRegex, bool caseSensitive,
    string? teamId = null, CancellationToken cancellationToken = default)
  {
    var builder = new StringBuilder("/posts/search?");
    builder.Append($"page=0&per_page={PageLimit}");
    builder.Append(useRegex ? "&is_regex=true" : "&is_regex=false");
    builder.Append(caseSensitive ? "&case_sensitive=true" : "&case_sensitive=false");
    if (!string.IsNullOrEmpty(channelId))
      builder.Append($"&channel_id={Uri.EscapeDataString(channelId)}");
    if (!string.IsNullOrEmpty(teamId))
      builder.Append($"&team_id={Uri.EscapeDataString(teamId)}");

    var payload = JsonSerializer.Serialize(new { term = pattern });
    using var content = new StringContent(payload, Encoding.UTF8, "application/json");

    using var response = await this.httpClient
      .PostAsync(builder.ToString(), content, cancellationToken);
    this.EnsureSuccess(response);

    var body = await response.Content.ReadAsStringAsync(cancellationToken);
    return JsonSerializer.Deserialize<MattermostSearchResult>(body) ?? new MattermostSearchResult();
  }

  private async Task<HttpResponseMessage> HttpGetAsync(string path, CancellationToken cancellationToken)
  {
    var response = await this.httpClient.GetAsync(path, cancellationToken);
    this.EnsureSuccess(response);
    return response;
  }

  private void EnsureSuccess(HttpResponseMessage response)
  {
    if (!response.IsSuccessStatusCode)
    {
      var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
      this.logger.LogError("Mattermost API error {status}: {body}", response.StatusCode, body);
      throw new HttpRequestException($"Mattermost API returned {(int)response.StatusCode}: {body}");
    }
  }

  #endregion

  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="options">Параметры подключения.</param>
  /// <param name="logger">Логгер.</param>
  public MattermostClient(IOptions<MattermostOptions> options, ILogger<MattermostClient> logger)
  {
    var config = options.Value;
    this.logger = logger;
    var client = new HttpClient
    {
      BaseAddress = new Uri(config.Url)
    };
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.Token}");
    this.httpClient = client;
  }

  /// <summary>
  /// Конструктор для тестирования.
  /// </summary>
  /// <param name="httpClient">Предконфигурированный http клиент.</param>
  /// <param name="logger">Логгер.</param>
  internal MattermostClient(HttpClient httpClient, ILogger<MattermostClient> logger)
  {
    this.httpClient = httpClient;
    this.logger = logger;
  }

  #endregion
}
