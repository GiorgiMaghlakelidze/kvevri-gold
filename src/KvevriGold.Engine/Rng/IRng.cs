namespace KvevriGold.Engine.Rng;

/// <summary>
/// Random number source used by every random decision in the engine (reel stops, reel-set choice).
/// The engine never creates its own randomness: an <see cref="IRng"/> is always injected, which keeps
/// the game logic deterministic and testable.
/// </summary>
public interface IRng
{
    /// <summary>Returns a uniformly distributed integer in [0, maxExclusive).</summary>
    int NextInt(int maxExclusive);
}
