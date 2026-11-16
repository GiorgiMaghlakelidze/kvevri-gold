namespace KvevriGold.Engine.Protocol;

/// <summary>Client spin request. During free spins the bet is ignored and the round's locked bet is used.</summary>
public sealed record SpinRequest(decimal Bet, string Variant = "96", string? SessionId = null);
