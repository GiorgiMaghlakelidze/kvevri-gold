using KvevriGold.Engine.Model;

namespace KvevriGold.Engine.Protocol;

/// <summary>Server response for a single spin (one base spin or one free spin).</summary>
public sealed class SpinResult
{
    public long RoundId { get; init; }
    public string Variant { get; init; } = "";
    public GameMode Mode { get; init; }
    public decimal Bet { get; init; }

    /// <summary>Amount debited for this spin (0 for free spins).</summary>
    public decimal Stake { get; init; }

    public string ReelSet { get; init; } = "";
    public int[] Stops { get; init; } = Array.Empty<int>();

    /// <summary>Initial board, one array per reel, top to bottom. Null when details are not captured.</summary>
    public string[][]? InitialBoard { get; init; }
    public string[][]? FinalBoard { get; init; }
    public IReadOnlyList<CascadeStepDto> Cascades { get; init; } = Array.Empty<CascadeStepDto>();

    public int Scatters { get; init; }
    public long ScatterWinCoins { get; init; }
    public long WaysWinCoins { get; init; }

    /// <summary>Total paid for this spin in coins, after the max-win cap.</summary>
    public long WinCoins { get; init; }
    public decimal Win { get; init; }

    public int FreeSpinsAwarded { get; init; }
    public int FreeSpinsRemaining { get; init; }
    public int FreeSpinsPlayed { get; init; }
    public int Multiplier { get; init; }

    public long RoundWinCoins { get; init; }
    public decimal RoundWin { get; init; }
    public bool MaxWinReached { get; init; }

    /// <summary>True when no free spins are pending: the next spin starts a new paid round.</summary>
    public bool RoundComplete { get; init; }
}

public sealed record CascadeStepDto(int Step, string[][]? Board, IReadOnlyList<WayWinDto> Wins, int Multiplier, long WinCoins);

public sealed record WayWinDto(string Symbol, int Length, int Ways, int PayPerWay, long WinCoins, int[][] Positions);
