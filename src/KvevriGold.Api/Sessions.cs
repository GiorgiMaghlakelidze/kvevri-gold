using System.Collections.Concurrent;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Protocol;

namespace KvevriGold.Api;

public sealed record SpinBody(decimal Bet, string? Variant = null, string? SessionId = null);

public sealed record SpinResponse(string SessionId, decimal Balance, SpinResult Result);

/// <summary>Per-player state. Demo only: in-memory, no auth, no persistence.</summary>
public sealed class Session
{
    public Session(string id, string variant)
    {
        Id = id;
        Variant = variant;
    }

    public string Id { get; }
    public string Variant { get; set; }
    public decimal Balance { get; set; } = SessionStore.StartingBalance;
    public GameContext Context { get; } = new();
    public object Sync { get; } = new();
}

public sealed class SessionStore
{
    public const decimal StartingBalance = 1000m;
    public const decimal MaxBet = 100m;

    private readonly ConcurrentDictionary<string, Session> _sessions = new();

    public Session GetOrCreate(string? id, string variant)
    {
        if (!string.IsNullOrWhiteSpace(id) && _sessions.TryGetValue(id, out var existing)) return existing;
        string newId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        return _sessions.GetOrAdd(newId, key => new Session(key, variant));
    }
}
