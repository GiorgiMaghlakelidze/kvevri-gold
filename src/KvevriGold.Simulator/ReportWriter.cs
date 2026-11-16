using System.Text;
using KvevriGold.Engine.Game;

namespace KvevriGold.Simulator;

public static class ReportWriter
{
    public static string ShortCount(long n) => n % 1_000_000 == 0 ? $"{n / 1_000_000}M" : n % 1000 == 0 ? $"{n / 1000}k" : n.ToString();

    private sealed record Metrics(double Rtp, double BaseRtp, double FreeRtp, double Sd, double Ci, double HitFreq,
        double TriggerFreq, double AvgFsWinX, double AvgFsSpins, double MaxWinX);

    private static Metrics Compute(KvevriGoldGame game, SimulationStats s)
    {
        double bet = game.Definition.CoinsPerBet;
        double n = s.Rounds;
        double rtp = s.TotalWin / bet / n;
        double meanSq = s.SumSquaresX / n;
        double sd = Math.Sqrt(Math.Max(0, meanSq - rtp * rtp));
        return new Metrics(rtp, s.BaseWin / bet / n, s.FreeWin / bet / n, sd, 1.96 * sd / Math.Sqrt(n),
            s.Hits / n, s.Triggers / n,
            s.Triggers == 0 ? 0 : s.FreeWin / bet / s.Triggers,
            s.Triggers == 0 ? 0 : (double)s.FreeSpinsPlayed / s.Triggers,
            s.MaxWinCoins / bet);
    }

    private static string P(double v) => $"{v * 100:0.000}%";
    private static string OneIn(double p) => p > 0 ? $"1 in {1 / p:N1}" : "never";

    public static string Summary(KvevriGoldGame game, SimulationStats s)
    {
        var m = Compute(game, s);
        double target = game.Definition.Variant.TargetRtp;
        return $"""
            RTP            {P(m.Rtp)}  (target {P(target)}, diff {(m.Rtp - target) * 100:+0.000;-0.000} pp, 95% CI ±{m.Ci * 100:0.000} pp)
              base game    {P(m.BaseRtp)}
              free spins   {P(m.FreeRtp)}
            Hit frequency  {P(m.HitFreq)} ({OneIn(m.HitFreq)})
            FS trigger     {OneIn(m.TriggerFreq)}  avg FS win {m.AvgFsWinX:0.00}x  avg FS played {m.AvgFsSpins:0.00}
            Std deviation  {m.Sd:0.000} (per round, in x bet)
            Max win        {m.MaxWinX:N2}x  (cap hits: {s.MaxWinCapHits:N0})
            """;
    }

    public static string Build(KvevriGoldGame game, SimOptions opt, SimulationStats s, TimeSpan elapsed)
    {
        var m = Compute(game, s);
        var def = game.Definition;
        double target = def.Variant.TargetRtp;
        double bet = def.CoinsPerBet;
        bool within = Math.Abs(m.Rtp - target) <= 0.003;
        var sb = new StringBuilder();
        sb.AppendLine($"# Kvevri Gold RTP simulation: variant {def.VariantId}");
        sb.AppendLine();
        sb.AppendLine($"Generated {DateTimeOffset.Now:yyyy-MM-dd HH:mm zzz} by `KvevriGold.Simulator`.");
        sb.AppendLine();
        sb.AppendLine("| Setting | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Paid rounds | {s.Rounds:N0} |");
        sb.AppendLine($"| Seed | {opt.Seed} ({opt.RngKind} RNG, {opt.Threads} threads) |");
        sb.AppendLine($"| Base reel sets | {string.Join(", ", def.BaseReelSets.Entries.Select(e => $"`{e.Item.Name}` w={e.Weight}"))} |");
        sb.AppendLine($"| Free spins reel sets | {string.Join(", ", def.FreeReelSets.Entries.Select(e => $"`{e.Item.Name}` w={e.Weight}"))} |");
        sb.AppendLine($"| Runtime | {elapsed.TotalSeconds:0.0}s ({s.Rounds / elapsed.TotalSeconds:N0} rounds/s) |");
        sb.AppendLine();
        sb.AppendLine("## Results");
        sb.AppendLine();
        sb.AppendLine("| Metric | Value |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| **RTP** | **{P(m.Rtp)}** |");
        sb.AppendLine($"| Target RTP | {P(target)} ({(within ? "within" : "OUTSIDE")} ±0.3 pp, diff {(m.Rtp - target) * 100:+0.000;-0.000} pp) |");
        sb.AppendLine($"| 95% confidence interval | {P(m.Rtp - m.Ci)} to {P(m.Rtp + m.Ci)} (±{m.Ci * 100:0.000} pp) |");
        sb.AppendLine($"| Base game RTP contribution | {P(m.BaseRtp)} |");
        sb.AppendLine($"| Free spins RTP contribution | {P(m.FreeRtp)} |");
        sb.AppendLine($"| Hit frequency (round wins > 0) | {P(m.HitFreq)} ({OneIn(m.HitFreq)}) |");
        sb.AppendLine($"| Free spins trigger frequency | {OneIn(m.TriggerFreq)} ({P(m.TriggerFreq)}) |");
        sb.AppendLine($"| Average free spins win | {m.AvgFsWinX:0.00}x bet |");
        sb.AppendLine($"| Average free spins played per trigger | {m.AvgFsSpins:0.00} (retriggers: {s.Retriggers:N0}) |");
        sb.AppendLine($"| Standard deviation per round | {m.Sd:0.000}x bet |");
        sb.AppendLine($"| Max win observed | {m.MaxWinX:N2}x bet |");
        sb.AppendLine($"| Max win cap ({def.Config.MaxWinX:N0}x) hits | {s.MaxWinCapHits:N0} |");
        sb.AppendLine();
        sb.AppendLine("## RTP by base reel set");
        sb.AppendLine();
        sb.AppendLine("Each round's base reel set is chosen by weight; the weighting is the main RTP tuning knob.");
        sb.AppendLine();
        sb.AppendLine("| Base reel set | Share of rounds | RTP when selected |");
        sb.AppendLine("|---|---|---|");
        foreach (var (name, v) in s.ByBaseReelSet.OrderBy(k => k.Key))
            sb.AppendLine($"| `{name}` | {P((double)v.Rounds / s.Rounds)} | {P(v.Win / bet / v.Rounds)} |");
        sb.AppendLine();
        sb.AppendLine("## Win distribution");
        sb.AppendLine();
        sb.AppendLine("| Round win (x bet) | Rounds | Frequency | 1 in | RTP contribution |");
        sb.AppendLine("|---|---:|---:|---:|---:|");
        for (int i = 0; i < s.Histogram.Length; i++)
        {
            long c = s.Histogram[i];
            double f = (double)c / s.Rounds;
            sb.AppendLine($"| {SimulationStats.BucketLabel(i)} | {c:N0} | {P(f)} | {(c > 0 ? (1 / f).ToString("N1") : "-")} | {P(s.HistogramWin[i] / bet / s.Rounds)} |");
        }
        sb.AppendLine();
        sb.AppendLine("Standard deviation is per paid round (base spin plus any free spins it triggered), in multiples of the bet. " +
                      "The confidence interval is RTP ± 1.96·σ/√N.");
        return sb.ToString();
    }
}
