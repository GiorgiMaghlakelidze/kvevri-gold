namespace KvevriGold.Engine.Rng;

/// <summary>
/// Replays a fixed list of values (modulo the requested range). Useful for forcing exact outcomes
/// in tests and for reproducing a reported spin from its logged RNG draws.
/// </summary>
public sealed class ScriptedRng : IRng
{
    private readonly int[] _values;
    private int _index;

    public ScriptedRng(params int[] values)
    {
        if (values.Length == 0) throw new ArgumentException("At least one value is required.", nameof(values));
        _values = values;
    }

    public int Draws => _index;

    public int NextInt(int maxExclusive)
    {
        int v = _values[_index % _values.Length];
        _index++;
        return ((v % maxExclusive) + maxExclusive) % maxExclusive;
    }
}
