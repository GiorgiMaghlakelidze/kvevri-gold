using KvevriGold.Engine.Features;
using KvevriGold.Engine.Math;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Protocol;
using KvevriGold.Engine.Rng;

namespace KvevriGold.Engine.Game;

/// <summary>
/// Generic cascading all-ways slot with scatter-triggered free spins. Holds no per-player state:
/// all state lives in the <see cref="GameContext"/> passed in, and all randomness comes from the
/// injected <see cref="IRng"/>, so one instance can serve many sessions and threads.
/// </summary>
public abstract class SlotGameBase
{
    protected SlotGameBase(GameDefinition definition) => Definition = definition;

    public GameDefinition Definition { get; }
    public string VariantId => Definition.VariantId;
    public virtual string Name => Definition.Config.Name;

    public SpinResult Spin(SpinRequest request, GameContext ctx, IRng rng) => Spin(ctx, request.Bet, rng, captureDetails: true);

    /// <summary>
    /// Plays the next spin: a paid base spin when no free spins are pending, otherwise the next free spin.
    /// </summary>
    /// <param name="ctx">Session state; updated in place.</param>
    /// <param name="bet">Total bet for a new round (ignored during free spins).</param>
    /// <param name="rng">Injected random source.</param>
    /// <param name="captureDetails">When false, boards and win breakdowns are not recorded (fast path for simulation).</param>
    public SpinResult Spin(GameContext ctx, decimal bet, IRng rng, bool captureDetails = true)
    {
        bool isFree = ctx.Mode == GameMode.FreeSpins;
        if (!isFree)
        {
            if (bet <= 0) throw new ArgumentOutOfRangeException(nameof(bet), "Bet must be positive.");
            ctx.StartRound(bet);
        }
        else
        {
            ctx.FreeSpinsRemaining--;
            ctx.FreeSpinsPlayed++;
        }
        ctx.TotalSpins++;

        var def = Definition;
        var board = new Board(def.Reels, def.Rows);
        ReelSet reelSet = (isFree ? def.FreeReelSets : def.BaseReelSets).Pick(rng);
        board.Spin(reelSet, rng);
        int[] stops = (int[])board.Stops.Clone();
        string[][]? initial = captureDetails ? board.ToCodes(def.Symbols) : null;

        List<CascadeStep>? steps = captureDetails ? new List<CascadeStep>() : null;
        ICascadeMultiplier multiplier = isFree ? def.FreeSpins.MultiplierFor(ctx) : FlatMultiplier.One;
        CascadeOutcome cascade = def.Cascades.Resolve(board, multiplier, steps, def.MaxWin.CapCoins - ctx.RoundWinCoins);

        int scatters = board.CountAll(def.Symbols.Scatter);
        long scatterWin = def.Paytable.ScatterPay(scatters);

        int awarded = isFree
            ? def.FreeSpins.TryRetrigger(ctx, scatters)
            : def.FreeSpins.TryTrigger(ctx, scatters);

        long win = def.MaxWin.Apply(ctx, cascade.Coins + scatterWin);
        ctx.RoundWinCoins += win;
        if (isFree) ctx.FreeSpinsWinCoins += win;

        decimal unit = ctx.RoundBet / def.CoinsPerBet;
        return new SpinResult
        {
            RoundId = ctx.RoundId,
            Variant = def.VariantId,
            Mode = isFree ? GameMode.FreeSpins : GameMode.Base,
            Bet = ctx.RoundBet,
            Stake = isFree ? 0m : ctx.RoundBet,
            ReelSet = reelSet.Name,
            Stops = stops,
            InitialBoard = initial,
            FinalBoard = captureDetails ? board.ToCodes(def.Symbols) : null,
            Cascades = steps is null ? Array.Empty<CascadeStepDto>() : steps.Select((s, i) => ToDto(s, i + 1)).ToArray(),
            Scatters = scatters,
            ScatterWinCoins = scatterWin,
            WaysWinCoins = cascade.Coins,
            WinCoins = win,
            Win = win * unit,
            FreeSpinsAwarded = ctx.MaxWinReached ? 0 : awarded,
            FreeSpinsRemaining = ctx.FreeSpinsRemaining,
            FreeSpinsPlayed = ctx.FreeSpinsPlayed,
            Multiplier = ctx.Multiplier,
            RoundWinCoins = ctx.RoundWinCoins,
            RoundWin = ctx.RoundWinCoins * unit,
            MaxWinReached = ctx.MaxWinReached,
            RoundComplete = ctx.FreeSpinsRemaining == 0,
        };
    }

    /// <summary>Plays a full round (base spin plus all triggered free spins) without capturing details.</summary>
    public RoundOutcome PlayRound(GameContext ctx, IRng rng, decimal bet = 1m)
    {
        SpinResult first = Spin(ctx, bet, rng, captureDetails: false);
        long baseWin = first.WinCoins;
        bool triggered = first.FreeSpinsAwarded > 0;
        int retriggers = 0;
        while (ctx.FreeSpinsRemaining > 0)
        {
            SpinResult fs = Spin(ctx, bet, rng, captureDetails: false);
            if (fs.FreeSpinsAwarded > 0) retriggers++;
        }
        return new RoundOutcome(first.ReelSet, baseWin, ctx.FreeSpinsWinCoins, ctx.RoundWinCoins, triggered, ctx.FreeSpinsPlayed, retriggers, ctx.MaxWinReached);
    }

    private CascadeStepDto ToDto(CascadeStep step, int index)
    {
        int rows = Definition.Rows;
        var wins = step.Wins.Select(w => new WayWinDto(
            Definition.Symbols.Code(w.Symbol), w.Length, w.Ways, w.PayPerWay, w.Coins * step.Multiplier,
            Enumerable.Range(0, Definition.Reels * rows)
                .Where(bit => (w.Mask & (1UL << bit)) != 0)
                .Select(bit => new[] { bit / rows, bit % rows })
                .ToArray())).ToArray();
        return new CascadeStepDto(index, step.Board, wins, step.Multiplier, step.Coins);
    }
}

/// <summary>Summary of one complete round, in coins (bet = CoinsPerBet coins).</summary>
public readonly record struct RoundOutcome(
    string BaseReelSet,
    long BaseWinCoins,
    long FreeSpinsWinCoins,
    long TotalWinCoins,
    bool FreeSpinsTriggered,
    int FreeSpinsPlayed,
    int Retriggers,
    bool MaxWinReached);
