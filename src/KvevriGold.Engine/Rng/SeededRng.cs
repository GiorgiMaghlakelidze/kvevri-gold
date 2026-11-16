using System.Numerics;

namespace KvevriGold.Engine.Rng;

/// <summary>
/// Fast deterministic PRNG (xoshiro256**, seeded through SplitMix64) for simulations and tests.
/// Not cryptographically secure: production play uses <see cref="CryptoRng"/>.
/// </summary>
public sealed class SeededRng : IRng
{
    private ulong _s0, _s1, _s2, _s3;

    public SeededRng(ulong seed)
    {
        Seed = seed;
        ulong x = seed;
        _s0 = SplitMix64(ref x);
        _s1 = SplitMix64(ref x);
        _s2 = SplitMix64(ref x);
        _s3 = SplitMix64(ref x);
    }

    public ulong Seed { get; }

    /// <summary>Derives an independent stream, e.g. one per simulator thread.</summary>
    public static SeededRng ForStream(ulong seed, int stream)
    {
        ulong x = seed ^ (0xD1B54A32D192ED03UL * (ulong)(stream + 1));
        return new SeededRng(SplitMix64(ref x));
    }

    public ulong NextULong()
    {
        ulong result = BitOperations.RotateLeft(_s1 * 5, 7) * 9;
        ulong t = _s1 << 17;
        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;
        _s2 ^= t;
        _s3 = BitOperations.RotateLeft(_s3, 45);
        return result;
    }

    /// <summary>Unbiased bounded integer (Lemire's multiply-and-reject method).</summary>
    public int NextInt(int maxExclusive)
    {
        if (maxExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
        ulong range = (ulong)maxExclusive;
        ulong threshold = (0UL - range) % range;
        while (true)
        {
            UInt128 product = (UInt128)NextULong() * range;
            if ((ulong)product >= threshold) return (int)(ulong)(product >> 64);
        }
    }

    private static ulong SplitMix64(ref ulong x)
    {
        ulong z = x += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
