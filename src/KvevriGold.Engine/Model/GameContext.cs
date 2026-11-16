namespace KvevriGold.Engine.Model;

public enum GameMode
{
    Base,
    FreeSpins,
}

/// <summary>
/// Player game state that must survive between spins (persisted per session by the API): pending free
/// spins, the progressive multiplier and the running round win used by the max-win cap.
/// A "round" is one paid base spin plus every free spin it triggers.
/// </summary>
public sealed class GameContext
{
    public GameMode Mode => FreeSpinsRemaining > 0 ? GameMode.FreeSpins : GameMode.Base;

    public long RoundId { get; set; }

    /// <summary>Bet locked in when the round started; free spins are played at this bet.</summary>
    public decimal RoundBet { get; set; }

    public int FreeSpinsRemaining { get; set; }
    public int FreeSpinsAwardedTotal { get; set; }
    public int FreeSpinsPlayed { get; set; }

    /// <summary>Progressive free-spins multiplier (persists across all free spins in the round).</summary>
    public int Multiplier { get; set; } = 1;

    public long RoundWinCoins { get; set; }
    public long FreeSpinsWinCoins { get; set; }
    public bool MaxWinReached { get; set; }

    public long TotalSpins { get; set; }

    public void StartRound(decimal bet)
    {
        RoundId++;
        RoundBet = bet;
        FreeSpinsRemaining = 0;
        FreeSpinsAwardedTotal = 0;
        FreeSpinsPlayed = 0;
        Multiplier = 1;
        RoundWinCoins = 0;
        FreeSpinsWinCoins = 0;
        MaxWinReached = false;
    }

    public GameContext Clone() => (GameContext)MemberwiseClone();
}
