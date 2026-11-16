using KvevriGold.Engine.Config;

namespace KvevriGold.Engine.Math;

/// <summary>Way pays and scatter tables, resolved to dense arrays for fast lookup.</summary>
public sealed class Paytable
{
    private readonly int[][] _wayPays;   // [symbol][count] in coins per way
    private readonly int[] _scatterPays; // [scatterCount] in coins
    private readonly int[] _awards;      // [scatterCount] free spins in base game
    private readonly int[] _retriggers;  // [scatterCount] free spins added during free spins

    public Paytable(GameConfig config, SymbolTable symbols)
    {
        Reels = config.Reels;
        MinWayLength = 3;
        _wayPays = new int[symbols.Count][];
        for (int s = 0; s < symbols.Count; s++) _wayPays[s] = new int[Reels + 1];
        foreach (var (code, pays) in config.Paytable)
        {
            byte id = symbols[code];
            if (id == symbols.Wild || id == symbols.Scatter)
                throw new InvalidDataException($"'{code}' cannot have way pays.");
            if (pays.Length != Reels - MinWayLength + 1)
                throw new InvalidDataException($"Paytable for '{code}' needs {Reels - MinWayLength + 1} entries.");
            for (int i = 0; i < pays.Length; i++) _wayPays[id][MinWayLength + i] = pays[i];
        }
        int maxScatter = config.Reels * config.Rows;
        _scatterPays = Dense(config.Scatter.Pays, maxScatter);
        _awards = Dense(config.Scatter.Awards, maxScatter);
        _retriggers = Dense(config.Scatter.Retriggers, maxScatter);
        MinScatterForFeature = config.Scatter.Awards.Count == 0 ? int.MaxValue : config.Scatter.Awards.Keys.Min();
    }

    public int Reels { get; }
    public int MinWayLength { get; }
    public int MinScatterForFeature { get; }

    /// <summary>Coins paid per way for <paramref name="count"/> consecutive reels of <paramref name="symbol"/>.</summary>
    public int WayPay(byte symbol, int count) => count < MinWayLength || count > Reels ? 0 : _wayPays[symbol][count];

    public int ScatterPay(int count) => _scatterPays[Clamp(count)];
    public int FreeSpinsAward(int count) => _awards[Clamp(count)];
    public int Retrigger(int count) => _retriggers[Clamp(count)];

    private int Clamp(int count) => System.Math.Clamp(count, 0, _scatterPays.Length - 1);

    /// <summary>
    /// Expands a sparse table to a dense array. Counts above the highest key reuse the highest key's value,
    /// so e.g. 6 scatters pays the same as 5.
    /// </summary>
    private static int[] Dense(Dictionary<int, int> table, int maxCount)
    {
        var dense = new int[maxCount + 1];
        if (table.Count == 0) return dense;
        int top = table.Keys.Max();
        for (int n = 0; n <= maxCount; n++)
        {
            int key = System.Math.Min(n, top);
            if (table.TryGetValue(key, out int v)) dense[n] = v;
            else if (n > top) dense[n] = table[top];
        }
        return dense;
    }
}
