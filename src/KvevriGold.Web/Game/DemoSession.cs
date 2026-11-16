using KvevriGold.Engine.Config;
using KvevriGold.Engine.Game;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Protocol;
using KvevriGold.Engine.Rng;

namespace KvevriGold.Web.Game;

/// <summary>
/// Client-side demo session: play-money balance, session statistics and the engine state
/// (<see cref="GameContext"/>) that carries free spins between spins. No real money is involved.
/// </summary>
public sealed class DemoSession
{
    public const decimal StartingBalance = 1000m;

    public static IReadOnlyList<decimal> BetSteps { get; } = new[] { 0.20m, 0.50m, 1m, 2m, 5m, 10m, 20m };

    private readonly Dictionary<string, KvevriGoldGame> _games;
    private IRng _rng;

    public DemoSession() : this(CryptoRng.Shared) { }

    public DemoSession(IRng rng)
    {
        _rng = rng;
        GameConfig config = GameConfigLoader.LoadEmbedded();
        _games = GameFactory.Variants.ToDictionary(v => v, v => GameFactory.Create(v, config));
        Config = config;
    }

    public GameConfig Config { get; }

    /// <summary>Set when the session replays a seeded RNG stream (for reproducible demos); null for live CSPRNG play.</summary>
    public ulong? Seed { get; private set; }

    /// <summary>Switches to a reproducible seeded RNG. Only allowed before the first spin.</summary>
    public bool TryUseSeed(ulong seed)
    {
        if (Context.TotalSpins > 0) return false;
        _rng = new SeededRng(seed);
        Seed = seed;
        return true;
    }
    public GameContext Context { get; } = new();

    public string Variant { get; private set; } = KvevriGold96.Id;
    public decimal Bet { get; private set; } = 1m;
    public decimal Balance { get; private set; } = StartingBalance;

    public bool InFreeSpins => Context.Mode == GameMode.FreeSpins;
    public KvevriGoldGame Game => _games[Variant];

    // Session statistics.
    public long PaidSpins { get; private set; }
    public long FreeSpinsPlayed { get; private set; }
    public decimal TotalBet { get; private set; }
    public decimal TotalWon { get; private set; }
    public decimal BiggestWin { get; private set; }
    public double? SessionRtp => TotalBet == 0 ? null : (double)(TotalWon / TotalBet);

    /// <summary>Copy of the session statistics, so the UI can reveal them after a spin's animation finishes.</summary>
    public SessionStats Snapshot() => new(PaidSpins, FreeSpinsPlayed, TotalBet, TotalWon, BiggestWin);

    public bool CanAfford => InFreeSpins || Balance >= Bet;

    public void SetBet(decimal bet)
    {
        if (InFreeSpins) return; // the round's bet is locked while free spins are pending
        if (!BetSteps.Contains(bet)) throw new ArgumentOutOfRangeException(nameof(bet));
        Bet = bet;
    }

    public void SetVariant(string variant)
    {
        if (InFreeSpins) return;
        if (!_games.ContainsKey(variant)) throw new ArgumentOutOfRangeException(nameof(variant));
        Variant = variant;
    }

    /// <summary>Plays the next spin: a paid base spin, or the next pending free spin at no cost.</summary>
    public SpinResult Spin()
    {
        if (!CanAfford) throw new InvalidOperationException("Not enough demo balance for this bet.");
        SpinResult result = Game.Spin(Context, Bet, _rng);
        Balance -= result.Stake;
        Balance += result.Win;
        if (result.Mode == GameMode.Base)
        {
            PaidSpins++;
            TotalBet += result.Stake;
        }
        else
        {
            FreeSpinsPlayed++;
        }
        TotalWon += result.Win;
        if (result.RoundComplete && result.RoundWin > BiggestWin) BiggestWin = result.RoundWin;
        return result;
    }

    /// <summary>Tops the play-money balance back up to the starting amount.</summary>
    public void RefillBalance()
    {
        if (Balance < StartingBalance) Balance = StartingBalance;
    }

    public void ResetStats()
    {
        PaidSpins = FreeSpinsPlayed = 0;
        TotalBet = TotalWon = BiggestWin = 0;
    }
}

public readonly record struct SessionStats(long PaidSpins, long FreeSpins, decimal TotalBet, decimal TotalWon, decimal BiggestWin)
{
    public double? Rtp => TotalBet == 0 ? null : (double)(TotalWon / TotalBet);
}
