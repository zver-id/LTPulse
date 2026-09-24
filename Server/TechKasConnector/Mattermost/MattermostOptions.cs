namespace TechKasConnector.Mattermost;

/// <summary>
/// Параметры для подключения к Mattermost.
/// </summary>
public class MattermostOptions
{
  /// <summary>
  /// Базовый адрес API (например, https://mm.example.com/api/v4/).
  /// </summary>
  public string Url { get; set; } = string.Empty;

  /// <summary>
  /// Префикс для ссылок на сообщения (например, https://talk.directum.ru/sluzhba-podderzhki-directum-spd).
  /// </summary>
  public string TeamUrl { get; set; } = string.Empty;

  /// <summary>
  /// Токен авторизации.
  /// </summary>
  public string Token { get; set; } = string.Empty;

  /// <summary>
  /// ИД каналов линии для поиска эскалаций.
  /// </summary>
  public List<string> LineChannels { get; set; } = new();

  /// <summary>
  /// ИД каналов разработчиков для поиска эскалаций.
  /// </summary>
  public List<string> DevChannels { get; set; } = new();
}
