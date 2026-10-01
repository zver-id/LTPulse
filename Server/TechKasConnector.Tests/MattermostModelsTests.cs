using System.Text.Json;
using TechKasConnector.Mattermost;

namespace TechKasConnector.Tests;

/// <summary>
/// Методы тестов моделей Mattermost.
/// </summary>
public class MattermostModelsTests
{
  [Test]
  public void SearchResult_OrderedPosts_FollowsOrderArray()
  {
    var result = new MattermostSearchResult();
    result.Posts["b"] = new MattermostPost { Id = "b", Message = "second" };
    result.Posts["a"] = new MattermostPost { Id = "a", Message = "first" };
    result.Order = new List<string> { "b", "a" };
    result.Total = 2;

    var ordered = result.OrderedPosts;

    Assert.AreEqual(2, ordered.Count);
    Assert.AreEqual("b", ordered[0].Id);
    Assert.AreEqual("a", ordered[1].Id);
  }

  [Test]
  public void SearchResult_OrderedPosts_SkipsMissingPost()
  {
    var result = new MattermostSearchResult();
    result.Posts["a"] = new MattermostPost { Id = "a", Message = "only" };
    result.Order = new List<string> { "a", "missing" };
    result.Total = 2;

    var ordered = result.OrderedPosts;

    Assert.AreEqual(1, ordered.Count);
    Assert.AreEqual("a", ordered[0].Id);
  }

  [Test]
  public void Post_Created_ConvertsMillisecondsToLocalDateTime()
  {
    var post = new MattermostPost { CreateAt = 1700000000000L };

    var expected = DateTimeOffset.FromUnixTimeMilliseconds(1700000000000L).LocalDateTime;

    Assert.AreEqual(expected, post.Created);
  }

  [Test]
  public void Models_DeserializeFromMattermostJson()
  {
    const string json = """
      {
        "posts": {
          "p1": {
            "id": "p1",
            "channel_id": "chan1",
            "team_id": "team1",
            "user_id": "user1",
            "message": "hello world",
            "create_at": 1700000000000
          }
        },
        "order": ["p1"],
        "total": 1
      }
      """;

    var result = JsonSerializer.Deserialize<MattermostSearchResult>(json);

    Assert.NotNull(result);
    Assert.AreEqual(1, result!.Total);
    Assert.AreEqual(1, result.OrderedPosts.Count);
    Assert.AreEqual("chan1", result.OrderedPosts[0].ChannelId);
    Assert.AreEqual("hello world", result.OrderedPosts[0].Message);
  }

  [Test]
  public void Channel_DeserializeFromMattermostJson()
  {
    const string json = """
      { "id": "c1", "team_id": "t1", "name": "general", "display_name": "General" }
      """;

    var channel = JsonSerializer.Deserialize<MattermostChannel>(json);

    Assert.NotNull(channel);
    Assert.AreEqual("c1", channel!.Id);
    Assert.AreEqual("general", channel.Name);
    Assert.AreEqual("General", channel.DisplayName);
  }
}
