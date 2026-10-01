using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TechKasConnector.Mattermost;

namespace TechKasConnector.Tests;

/// <summary>
/// Методы тестов MattermostClient.
/// </summary>
public class MattermostClientTests
{
  private static MattermostClient CreateClient(HttpMessageHandler handler, string baseUrl = "https://mm.test")
  {
    var http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    http.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
    return new MattermostClient(http, NullLogger<MattermostClient>.Instance);
  }

  private static string SearchResponseJson(params (string id, string message, string channelId)[] posts)
  {
    var dict = new Dictionary<string, object>();
    var order = new List<string>();
    foreach (var (id, message, channelId) in posts)
    {
      dict[id] = new
      {
        id,
        message,
        channel_id = channelId,
        team_id = "team1",
        user_id = "user1",
        create_at = 1700000000000L
      };
      order.Add(id);
    }
    return JsonSerializer.Serialize(new { posts = dict, order, total = posts.Length });
  }

  [Test]
  public async Task SearchChannelByRegex_ReturnsOrderedPosts()
  {
    var json = SearchResponseJson(
      ("p1", "ticket #123 opened", "chan1"),
      ("p2", "ticket #456 opened", "chan1"));

    using var handler = new StubHandler(json);
    var client = CreateClient(handler);

    var result = await client.SearchChannelByRegex("chan1", @"ticket #\d+");

    Assert.AreEqual(2, result.Count);
    Assert.AreEqual("p1", result[0].Id);
    Assert.AreEqual("p2", result[1].Id);
    Assert.AreEqual("ticket #123 opened", result[0].Message);
    StringAssert.Contains("is_regex=true", handler.LastRequestUri!.Query);
    StringAssert.Contains("channel_id=chan1", handler.LastRequestUri.Query);
  }

  [Test]
  public async Task SearchChannelByRegex_SendsCaseInsensitiveByDefault()
  {
    using var handler = new StubHandler(SearchResponseJson(("p1", "x", "chan1")));
    var client = CreateClient(handler);

    await client.SearchChannelByRegex("chan1", "abc");

    StringAssert.Contains("case_sensitive=false", handler.LastRequestUri!.Query);
  }

  [Test]
  public async Task SearchChannelByRegex_CaseSensitive_SendsTrue()
  {
    using var handler = new StubHandler(SearchResponseJson(("p1", "x", "chan1")));
    var client = CreateClient(handler);

    await client.SearchChannelByRegex("chan1", "abc", caseSensitive: true);

    StringAssert.Contains("case_sensitive=true", handler.LastRequestUri!.Query);
  }

  [Test]
  public async Task SearchByRegex_WithoutChannel_OmitsChannelId()
  {
    using var handler = new StubHandler(SearchResponseJson(("p1", "x", "chan1")));
    var client = CreateClient(handler);

    await client.SearchByRegex(@"\d+", teamId: "team1");

    var query = handler.LastRequestUri!.Query;
    StringAssert.DoesNotContain("channel_id", query);
    StringAssert.Contains("team_id=team1", query);
    StringAssert.Contains("is_regex=true", query);
  }

  [Test]
  public async Task SearchChannel_UsesNonRegexMode()
  {
    using var handler = new StubHandler(SearchResponseJson(("p1", "x", "chan1")));
    var client = CreateClient(handler);

    await client.SearchChannel("chan1", "hello");

    StringAssert.Contains("is_regex=false", handler.LastRequestUri!.Query);
    StringAssert.Contains("\"term\":\"hello\"", handler.LastRequestBody!);
  }

  [Test]
  public async Task SearchChannelByRegex_SendsTermInBody()
  {
    using var handler = new StubHandler(SearchResponseJson(("p1", "x", "chan1")));
    var client = CreateClient(handler);

    await client.SearchChannelByRegex("chan1", @"^BUG-\d+$");

    StringAssert.Contains("\"term\":", handler.LastRequestBody!);
    StringAssert.Contains("BUG-", handler.LastRequestBody);
  }

  [Test]
  public async Task GetTeamChannels_ReturnsChannels()
  {
    var json = JsonSerializer.Serialize(new[]
    {
      new { id = "c1", team_id = "t1", name = "general", display_name = "General" }
    });
    using var handler = new StubHandler(json);
    var client = CreateClient(handler);

    var channels = await client.GetTeamChannels("t1");

    Assert.AreEqual(1, channels.Count);
    Assert.AreEqual("c1", channels[0].Id);
    Assert.AreEqual("general", channels[0].Name);
    StringAssert.EndsWith("/teams/t1/channels", handler.LastRequestUri!.PathAndQuery);
  }

  [Test]
  public async Task SearchChannelByRegex_ThrowsOnErrorStatus()
  {
    using var handler = new StubHandler("not found", statusCode: HttpStatusCode.NotFound);
    var client = CreateClient(handler);

    Assert.ThrowsAsync<HttpRequestException>(
      async () => await client.SearchChannelByRegex("chan1", "x"));
  }

  [Test]
  public async Task SearchByRegex_EmptyResult_ReturnsEmptyList()
  {
    using var handler = new StubHandler(JsonSerializer.Serialize(new { posts = new Dictionary<string, object>(), order = new List<string>(), total = 0 }));
    var client = CreateClient(handler);

    var result = await client.SearchByRegex("nothing");

    Assert.IsEmpty(result);
  }
}

/// <summary>
/// Заглушка http обработчика для тестов.
/// </summary>
internal class StubHandler : HttpMessageHandler
{
  private readonly string responseBody;
  private readonly HttpStatusCode statusCode;

  public Uri? LastRequestUri { get; private set; }
  public string? LastRequestBody { get; private set; }

  public StubHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
  {
    this.responseBody = responseBody;
    this.statusCode = statusCode;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    this.LastRequestUri = request.RequestUri;
    if (request.Content != null)
      this.LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
    return new HttpResponseMessage(this.statusCode)
    {
      Content = new StringContent(this.responseBody, Encoding.UTF8, "application/json")
    };
  }
}
