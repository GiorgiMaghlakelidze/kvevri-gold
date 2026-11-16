using KvevriGold.Engine.Config;

namespace KvevriGold.Engine.Math;

/// <summary>Maps symbol codes to compact byte ids used on the board and reel strips.</summary>
public sealed class SymbolTable
{
    private readonly Dictionary<string, byte> _byCode;
    private readonly SymbolConfig[] _symbols;

    public SymbolTable(IReadOnlyList<SymbolConfig> symbols)
    {
        if (symbols.Count == 0 || symbols.Count > 255) throw new InvalidDataException("Between 1 and 255 symbols are required.");
        _symbols = symbols.ToArray();
        _byCode = new Dictionary<string, byte>(StringComparer.Ordinal);
        Wild = Scatter = byte.MaxValue;
        for (int i = 0; i < _symbols.Length; i++)
        {
            SymbolConfig s = _symbols[i];
            if (!_byCode.TryAdd(s.Code, (byte)i)) throw new InvalidDataException($"Duplicate symbol code '{s.Code}'.");
            if (s.Kind == SymbolKind.Wild) Wild = Wild == byte.MaxValue ? (byte)i : throw new InvalidDataException("Only one wild symbol is supported.");
            if (s.Kind == SymbolKind.Scatter) Scatter = Scatter == byte.MaxValue ? (byte)i : throw new InvalidDataException("Only one scatter symbol is supported.");
        }
        if (Wild == byte.MaxValue) throw new InvalidDataException("A wild symbol is required.");
        if (Scatter == byte.MaxValue) throw new InvalidDataException("A scatter symbol is required.");
        PayingSymbols = Enumerable.Range(0, _symbols.Length)
            .Where(i => _symbols[i].Kind is SymbolKind.High or SymbolKind.Low)
            .Select(i => (byte)i)
            .ToArray();
    }

    public byte Wild { get; }
    public byte Scatter { get; }
    public int Count => _symbols.Length;

    /// <summary>Symbols that can form way wins (everything except wild and scatter).</summary>
    public byte[] PayingSymbols { get; }

    public byte this[string code] =>
        _byCode.TryGetValue(code, out byte id) ? id : throw new KeyNotFoundException($"Unknown symbol code '{code}'.");

    public string Code(byte id) => _symbols[id].Code;
    public SymbolConfig Get(byte id) => _symbols[id];
    public IReadOnlyList<SymbolConfig> All => _symbols;
}
