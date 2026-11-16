using System.Security.Cryptography;

namespace KvevriGold.Engine.Rng;

/// <summary>
/// Cryptographically secure RNG backed by the OS CSPRNG
/// (<see cref="RandomNumberGenerator.GetInt32(int)"/>, which is unbiased). Used for live play.
/// </summary>
public sealed class CryptoRng : IRng
{
    public static readonly CryptoRng Shared = new();

    public int NextInt(int maxExclusive) => RandomNumberGenerator.GetInt32(maxExclusive);
}
