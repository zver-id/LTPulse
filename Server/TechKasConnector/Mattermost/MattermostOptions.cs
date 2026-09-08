namespace TechKasConnector.Mattermost;

/// <summary>
/// Параметры для подключения к Mattermost.
/// </summary>
public class MattermostOptions
{
  /// <summary>
  /// Базовый адрес API (например, https://mm.example.com).
  /// </summary>
  public string Url { get; set; } = string.Empty;

  /// <summary>
  /// Токен авторизации.
  /// </summary>
  public string Token { get; set; } = string.Empty;
}
