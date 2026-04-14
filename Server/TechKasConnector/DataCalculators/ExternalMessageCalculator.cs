using System.Net;
using System.Text.RegularExpressions;

namespace TechKasConnector.DataCalculators;

public class ExternalMessageCalculator
{
  public HttpClient httpClient {get; set;}
  
  public static async Task<FormUrlEncodedContent> GetBodyForAuthorizeAsync(HttpClient httpClient, string email, string password)
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
  
  public static string ExtractAntiForgeryToken(string htmlBody)
  {
    var requestVerificationTokenMatch =
      Regex.Match(htmlBody, $@"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>");

    if (requestVerificationTokenMatch.Success)
      return requestVerificationTokenMatch.Groups[1].Captures[0].Value;

    return string.Empty;
  }

  public async Task<string> GetHtml()
  {
    this.httpClient.BaseAddress = new Uri("https://examle.ru");
    var formContent = await GetBodyForAuthorizeAsync(this.httpClient,
      "usermanr@mail.ru", "Qwerrty");
    var loginResponse = await this.httpClient.PostAsync("/account/Login", formContent);
    
    var profilePage = await this.httpClient.GetAsync("/Admin/Home/Analytics");
    var profileHtml = await profilePage.Content.ReadAsStringAsync();
    return profileHtml;
  }

  public ExternalMessageCalculator()
  {
    this.httpClient = new HttpClient();
  }
}