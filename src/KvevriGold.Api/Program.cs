using System.Text.Json.Serialization;
using KvevriGold.Api;
using KvevriGold.Engine.Config;
using KvevriGold.Engine.Game;
using KvevriGold.Engine.Rng;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

GameConfig config = GameConfigLoader.LoadEmbedded();
var games = GameFactory.Variants.ToDictionary(v => v, v => (SlotGameBase)GameFactory.Create(v, config));
builder.Services.AddSingleton<IReadOnlyDictionary<string, SlotGameBase>>(games);
builder.Services.AddSingleton<IRng>(CryptoRng.Shared);
builder.Services.AddSingleton<SessionStore>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/config", (string? variant) =>
{
    string v = variant ?? "96";
    if (!games.TryGetValue(v, out var game)) return Results.BadRequest(new { error = $"Unknown variant '{v}'. Use 94 or 96." });
    return Results.Ok(ConfigView.From(game));
});

app.MapPost("/spin", (SpinBody body, SessionStore sessions, IRng rng) =>
{
    if (body.Bet is <= 0 or > SessionStore.MaxBet)
        return Results.BadRequest(new { error = $"Bet must be between 0 and {SessionStore.MaxBet}." });

    Session session = sessions.GetOrCreate(body.SessionId, body.Variant ?? "96");
    lock (session.Sync)
    {
        // A session keeps its variant while free spins are pending; otherwise the client may switch.
        string variant = session.Context.Mode == KvevriGold.Engine.Model.GameMode.FreeSpins ? session.Variant : body.Variant ?? session.Variant;
        if (!games.TryGetValue(variant, out var game))
            return Results.BadRequest(new { error = $"Unknown variant '{variant}'. Use 94 or 96." });
        session.Variant = variant;

        bool paid = session.Context.Mode == KvevriGold.Engine.Model.GameMode.Base;
        if (paid && session.Balance < body.Bet)
            return Results.BadRequest(new { error = "Insufficient demo balance.", session.Balance });

        var result = game.Spin(session.Context, body.Bet, rng);
        session.Balance += result.Win - result.Stake;
        return Results.Ok(new SpinResponse(session.Id, session.Balance, result));
    }
});

app.Run();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;
