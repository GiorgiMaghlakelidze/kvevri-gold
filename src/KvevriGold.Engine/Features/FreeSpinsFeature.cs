using KvevriGold.Engine.Config;
using KvevriGold.Engine.Math;
using KvevriGold.Engine.Model;

namespace KvevriGold.Engine.Features;

/// <summary>
/// "Rtveli" free spins: 3+ scatters in the base game award free spins; 3+ scatters during free spins
/// add more. Every winning cascade during free spins raises a multiplier that never resets until the
/// feature ends (capped at <see cref="FreeSpinsConfig.MultiplierMax"/>).
/// </summary>
public sealed class FreeSpinsFeature
{
    private readonly Paytable _paytable;
    private readonly FreeSpinsConfig _config;

    public FreeSpinsFeature(Paytable paytable, FreeSpinsConfig config)
    {
        if (config.MultiplierStart < 1 || config.MultiplierMax < config.MultiplierStart || config.MultiplierStep < 0)
            throw new InvalidDataException("Invalid free spins multiplier settings.");
        _paytable = paytable;
        _config = config;
    }

    public FreeSpinsConfig Config => _config;

    /// <summary>Base game: awards free spins for the scatter count. Returns spins awarded (0 if none).</summary>
    public int TryTrigger(GameContext ctx, int scatterCount)
    {
        int award = _paytable.FreeSpinsAward(scatterCount);
        if (award <= 0) return 0;
        ctx.FreeSpinsRemaining = award;
        ctx.FreeSpinsAwardedTotal = award;
        ctx.Multiplier = _config.MultiplierStart;
        return award;
    }

    /// <summary>During free spins: adds spins for the scatter count. Returns spins added.</summary>
    public int TryRetrigger(GameContext ctx, int scatterCount)
    {
        int extra = _paytable.Retrigger(scatterCount);
        if (extra <= 0) return 0;
        ctx.FreeSpinsRemaining += extra;
        ctx.FreeSpinsAwardedTotal += extra;
        return extra;
    }

    /// <summary>Multiplier view over the context, used by the cascade resolver during free spins.</summary>
    public ICascadeMultiplier MultiplierFor(GameContext ctx) => new ProgressiveMultiplier(ctx, _config);

    private sealed class ProgressiveMultiplier : ICascadeMultiplier
    {
        private readonly GameContext _ctx;
        private readonly FreeSpinsConfig _config;

        public ProgressiveMultiplier(GameContext ctx, FreeSpinsConfig config)
        {
            _ctx = ctx;
            _config = config;
        }

        public int Current => _ctx.Multiplier;

        public void OnWinningStep() =>
            _ctx.Multiplier = System.Math.Min(_config.MultiplierMax, _ctx.Multiplier + _config.MultiplierStep);
    }
}
