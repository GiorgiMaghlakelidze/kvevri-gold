namespace KvevriGold.Simulator;

/// <summary>Per-thread accumulator of round statistics; merged at the end. Wins are in coins.</summary>
public sealed class SimulationStats
{
    /// <summary>Lower edges of the win histogram buckets in multiples of the bet (first bucket = no win).</summary>
    public static readonly double[] BucketEdges = { 0, 1, 2, 5, 10, 20, 50, 100, 250, 500, 1000, 2500 };

    public long Rounds;
    public long TotalWin;
    public long BaseWin;
    public long FreeWin;
    public double SumSquaresX; // sum of (win in x bet)^2
    public long Hits;
    public long Triggers;
    public long FreeSpinsPlayed;
    public long Retriggers;
    public long MaxWinCapHits;
    public long MaxWinCoins;
    public long[] Histogram = new long[BucketEdges.Length + 2]; // [0]=no win, then (0,1), [1,2) ... , last = capped
    public long[] HistogramWin = new long[BucketEdges.Length + 2];
    public Dictionary<string, (long Rounds, long Win)> ByBaseReelSet = new();

    public void Add(string baseReelSet, long baseWin, long freeWin, long total, bool triggered, int fsPlayed, int retriggers, bool capped, int coinsPerBet)
    {
        Rounds++;
        TotalWin += total;
        BaseWin += baseWin;
        FreeWin += freeWin;
        double x = (double)total / coinsPerBet;
        SumSquaresX += x * x;
        if (total > 0) Hits++;
        if (triggered) { Triggers++; FreeSpinsPlayed += fsPlayed; Retriggers += retriggers; }
        if (capped) MaxWinCapHits++;
        if (total > MaxWinCoins) MaxWinCoins = total;
        int b = Bucket(x, total, capped);
        Histogram[b]++;
        HistogramWin[b] += total;
        ByBaseReelSet.TryGetValue(baseReelSet, out var r);
        ByBaseReelSet[baseReelSet] = (r.Rounds + 1, r.Win + total);
    }

    private static int Bucket(double x, long total, bool capped)
    {
        if (total == 0) return 0;
        if (capped) return BucketEdges.Length + 1;
        for (int i = BucketEdges.Length - 1; i >= 0; i--) if (x >= BucketEdges[i]) return i + 1;
        return 1;
    }

    public void Merge(SimulationStats o)
    {
        Rounds += o.Rounds; TotalWin += o.TotalWin; BaseWin += o.BaseWin; FreeWin += o.FreeWin;
        SumSquaresX += o.SumSquaresX; Hits += o.Hits; Triggers += o.Triggers; FreeSpinsPlayed += o.FreeSpinsPlayed;
        Retriggers += o.Retriggers; MaxWinCapHits += o.MaxWinCapHits; MaxWinCoins = Math.Max(MaxWinCoins, o.MaxWinCoins);
        for (int i = 0; i < Histogram.Length; i++) { Histogram[i] += o.Histogram[i]; HistogramWin[i] += o.HistogramWin[i]; }
        foreach (var (k, v) in o.ByBaseReelSet)
        {
            ByBaseReelSet.TryGetValue(k, out var r);
            ByBaseReelSet[k] = (r.Rounds + v.Rounds, r.Win + v.Win);
        }
    }

    public static string BucketLabel(int i, int maxWinX)
    {
        if (i == 0) return "0 (no win)";
        if (i == BucketEdges.Length + 1) return $"{maxWinX}x (max win cap hit)";
        double lo = BucketEdges[i - 1];
        string hi = i < BucketEdges.Length ? $"{BucketEdges[i]:0}x" : $"{maxWinX}x";
        return lo == 0 ? $"(0x, {hi})" : $"[{lo:0}x, {hi})";
    }
}
