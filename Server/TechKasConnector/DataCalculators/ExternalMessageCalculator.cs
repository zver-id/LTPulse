using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace TechKasConnector.DataCalculators;

/// <summary>
/// Расчет баллов внешних сообщений.
/// </summary>
public class ExternalMessageCalculator
{
  #region Поля и свойства
  
  /// <summary>
  /// Http клиент для получения данных.
  /// </summary>
  private HttpClient HttpClient {get; set;}
  
  /// <summary>
  /// Url сайта.
  /// </summary>
  private readonly string baseUrl;
  
  /// <summary>
  /// Имя пользователя.
  /// </summary>
  private readonly string userName;
  
  /// <summary>
  /// Пароль пользователя.
  /// </summary>
  private readonly string password; 
  
  #endregion
  
  #region Методы

  /// <summary>
  /// Получить набранные сотрудниками баллы внешних сообщений.
  /// </summary>
  /// <returns>Сотрудник: количество набранных балов.</returns>
  public async Task<Dictionary<string, int>> GetEmployeeScores()
  {
    var doc = new HtmlDocument();
    var html = await this.GetAnalyticsHtml();
    doc.LoadHtml(html);
    
    string spdChapterName = "Текущий месяц (для СПД)";
    HtmlNode targetTable = doc.DocumentNode
      .SelectSingleNode($"//table[preceding::h3[contains(normalize-space(), '{spdChapterName}')]][2]");

    var scoresList = ExtractTableData(targetTable);
    var employeeScores = new Dictionary<string, int>();
    foreach (var employeeScore in scoresList)
    {
      // в структуре страницы получается, что первое згачение - имя сотрудника, а последнее, №5 - итого.
      // но первая строка содержит заголовки и не нужна.
      int score;
      if (int.TryParse(employeeScore[4], out score))
      {
        employeeScores[SwapName(employeeScore[0])] = score;
      }
    }
    return employeeScores;
  }

  /// <summary>
  /// Аутентифицироваться на сайте.
  /// </summary>
  private async Task Login()
  {
    this.HttpClient.BaseAddress = new Uri(this.baseUrl);
    var formContent = await GetBodyForAuthorizeAsync(this.HttpClient,
      this.userName, this.password);
    var loginResponse = await this.HttpClient.PostAsync("/account/Login", formContent);
  }
  
  /// <summary>
  /// Получить html код страницы с аналитикой.
  /// </summary>
  /// <returns>Html код страницы с аналитикой.</returns>
  private async Task<string> GetAnalyticsHtml()
  {
    await this.Login();
    HttpResponseMessage analyticsPage = await this.HttpClient.GetAsync("/Admin/Home/Analytics");
    var analyticsContent = await analyticsPage.Content.ReadAsStringAsync();
    return WebUtility.HtmlDecode(analyticsContent);
  }
  
  /// <summary>
  /// Поменять имя и фамилию местами. Нужно пока как костыль, так как имя и фамилия на сайте написаны наоборот.
  /// </summary>
  /// <param name="fullName">Имя, фамилия.</param>
  /// <returns>Фамилия, имя.</returns>
  private static string SwapName(string fullName)
  {
    var parts = fullName.Split(' ');
    if (parts.Length == 2)
      return $"{parts[1]} {parts[0]}";
    return fullName;
  }
  
  /// <summary>
  /// Получить форму для аутентификации на сайте.
  /// </summary>
  /// <param name="httpClient">http клиент.</param>
  /// <param name="email">Логин.</param>
  /// <param name="password">Пароль.</param>
  /// <returns>Форма для аутентификации на сайте.</returns>
  private static async Task<FormUrlEncodedContent> GetBodyForAuthorizeAsync(HttpClient httpClient, string email, string password)
  {
    var loginModal = await httpClient.GetAsync("/account/signin");
    var modal = await loginModal.Content.ReadAsStringAsync();

    var antiforgeryToken = ExtractAntiForgeryToken(modal);  

    return new FormUrlEncodedContent(new[]
    {
      new KeyValuePair<string, string>("__RequestVerificationToken", antiforgeryToken),
      new KeyValuePair<string, string>("RememberMe", "false"),
      new KeyValuePair<string, string>("Email", email),
      new KeyValuePair<string, string>("Password", password),
      new KeyValuePair<string, string>("returnUrl", "/"),
    });
  }
  
  /// <summary>
  /// Получить AntiForgery Token из ответа.
  /// </summary>
  /// <param name="htmlBody">html ответа.</param>
  /// <returns>AntiForgery Token.</returns>
  private static string ExtractAntiForgeryToken(string htmlBody)
  {
    var requestVerificationTokenMatch =
      Regex.Match(htmlBody, $@"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>");

    if (requestVerificationTokenMatch.Success)
      return requestVerificationTokenMatch.Groups[1].Captures[0].Value;

    return string.Empty;
  }
  
  /// <summary>
  /// Получить данные таблицы из html.
  /// </summary>
  /// <param name="table">Объект таблицы.</param>
  /// <returns>Извлеченные данные.</returns>
  private static List<List<string>> ExtractTableData(HtmlNode table)
  {
    var result = new List<List<string>>();
    var rows = table.SelectNodes(".//tr");
    if (rows == null) return result;
        
    foreach (var row in rows)
    {
      var cells = row.SelectNodes(".//td | .//th");
      if (cells != null)
      {
        var rowData = cells.Select(c => c.InnerText?.Trim()).ToList();
        if (rowData.Any())
        {
          result.Add(rowData);
        }
      }
    }
    return result;
  }
  
  #endregion

  #region Конструкторы
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="options">Параметры для подключения.</param>
  public ExternalMessageCalculator(IOptions<ClubDetailsOptions> options)
  {
    var clubConfig = options.Value;
    this.HttpClient = new HttpClient();
    this.baseUrl = clubConfig.Url;
    this.userName = clubConfig.Name;
    this.password = clubConfig.Password;
  }
  #endregion

}

/// <summary>
/// Параметры для подключения к сайту.
/// </summary>
public class ClubDetailsOptions
{
  public string Url { get; set; }
  public string Name { get; set; }
  public string Password { get; set; }
}
