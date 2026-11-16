namespace KvevriGold.Engine.Math;

/// <summary>A single circular reel strip.</summary>
public sealed class ReelStrip
{
    private readonly byte[] _symbols;

    public ReelStrip(byte[] symbols)
    {
        if (symbols.Length == 0) throw new ArgumentException("A reel strip cannot be empty.", nameof(symbols));
        _symbols = symbols;
    }

    public int Length => _symbols.Length;

    /// <summary>Symbol at <paramref name="index"/>, wrapping around the strip in both directions.</summary>
    public byte this[int index]
    {
        get
        {
            int i = index % _symbols.Length;
            return _symbols[i < 0 ? i + _symbols.Length : i];
        }
    }

    public int CountOf(byte symbol) => _symbols.Count(s => s == symbol);
    public ReadOnlySpan<byte> Symbols => _symbols;
}

/// <summary>A named set of strips, one per reel.</summary>
public sealed class ReelSet
{
    public ReelSet(string name, ReelStrip[] strips)
    {
        Name = name;
        Strips = strips;
    }

    public string Name { get; }
    public ReelStrip[] Strips { get; }
    public int Reels => Strips.Length;
}
