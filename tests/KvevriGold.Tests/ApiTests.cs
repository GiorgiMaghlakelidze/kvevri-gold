using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace KvevriGold.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Config_ReturnsRulesWithoutReelStrips()
    {
        var json = await _client.GetStringAsync("/config?variant=94");
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Kvevri Gold", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal(243, doc.RootElement.GetProperty("ways").GetInt32());
        Assert.False(doc.RootElement.TryGetProperty("reelSets", out _));
    }

    [Fact]
    public async Task Spin_KeepsSessionStateAndBalance()
    {
        var first = await _client.PostAsJsonAsync("/spin", new { bet = 1.0m, variant = "96" });
        first.EnsureSuccessStatusCode();
        using var a = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
        string session = a.RootElement.GetProperty("sessionId").GetString()!;
        decimal balance = a.RootElement.GetProperty("balance").GetDecimal();
        var result = a.RootElement.GetProperty("result");
        Assert.Equal(1000m - 1m + result.GetProperty("win").GetDecimal(), balance);
        Assert.Equal(5, result.GetProperty("initialBoard").GetArrayLength());

        var second = await _client.PostAsJsonAsync("/spin", new { bet = 1.0m, sessionId = session });
        using var b = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        Assert.Equal(session, b.RootElement.GetProperty("sessionId").GetString());
    }

    [Fact]
    public async Task Spin_RejectsInvalidBetAndVariant()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsJsonAsync("/spin", new { bet = 0m })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _client.PostAsJsonAsync("/spin", new { bet = 1m, variant = "99" })).StatusCode);
    }
}
