using KvevriGold.Engine.Model;

namespace KvevriGold.Engine.Features;

/// <summary>
/// Caps the total win of a round at MaxWinX times the bet. When the cap is hit the remaining free spins
/// are forfeited and the round ends immediately.
/// </summary>
public sealed class MaxWinCap
{
    public MaxWinCap(long capCoins) => CapCoins = capCoins;

    public long CapCoins { get; }

    /// <summary>Returns how much of <paramref name="coins"/> may still be paid in this round.</summary>
    public long Apply(GameContext ctx, long coins)
    {
        long room = CapCoins - ctx.RoundWinCoins;
        if (coins < room) return coins;
        ctx.MaxWinReached = true;
        ctx.FreeSpinsRemaining = 0;
        return System.Math.Max(0, room);
    }
}
