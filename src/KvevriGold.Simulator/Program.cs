using System.Diagnostics;
using System.Globalization;
using System.Text;
using KvevriGold.Engine.Config;
using KvevriGold.Engine.Game;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Rng;
using KvevriGold.Simulator;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

SimOptions opt;
try { opt = SimOptions.Parse(args); }
catch (Exception ex) { Console.Error.WriteLine(ex.Message); SimOptions.PrintUsage(); return 2; }
if (opt.Help) { SimOptions.PrintUsage(); return 0; }

GameConfig config = opt.ConfigPath is null ? GameConfigLoader.LoadEmbedded() : GameConfigLoader.LoadFile(opt.ConfigPath);
KvevriGoldGame game = GameFactory.Create(opt.Variant, config);
int coinsPerBet = game.Definition.CoinsPerBet;

Console.WriteLine($"Kvevri Gold simulator | variant {opt.Variant} | {opt.Spins:N0} spins | seed {opt.Seed} | {opt.Threads} threads | rng {opt.RngKind}");
var sw = Stopwatch.StartNew();

var perThread = new SimulationStats[opt.Threads];
long done = 0;
using var progressCts = new CancellationTokenSource();
var progress = Task.Run(async () =>
{
    while (!progressCts.IsCancelled())
    {
        try { await Task.Delay(5000, progressCts.Token); } catch (TaskCanceledException) { break; }
        long d = Interlocked.Read(ref done);
        if (!opt.Quiet) Console.WriteLine($"  {d:N0} / {opt.Spins:N0} spins ({100.0 * d / opt.Spins:0.0}%), {sw.Elapsed.TotalSeconds:0}s");
    }
});

Parallel.For(0, opt.Threads, new ParallelOptions { MaxDegreeOfParallelism = opt.Threads }, t =>
{
    long count = opt.Spins / opt.Threads + (t < opt.Spins % opt.Threads ? 1 : 0);
    IRng rng = opt.RngKind == "crypto" ? new CryptoRng() : SeededRng.ForStream(opt.Seed, t);
    var stats = new SimulationStats();
    var ctx = new GameContext();
    for (long i = 0; i < count; i++)
    {
        RoundOutcome r = game.PlayRound(ctx, rng);
        stats.Add(r.BaseReelSet, r.BaseWinCoins, r.FreeSpinsWinCoins, r.TotalWinCoins, r.FreeSpinsTriggered, r.FreeSpinsPlayed, r.Retriggers, r.MaxWinReached, coinsPerBet);
        if ((i & 0xFFFF) == 0xFFFF) Interlocked.Add(ref done, 0x10000);
    }
    perThread[t] = stats;
});
progressCts.Cancel();
await progress;
sw.Stop();

var total = new SimulationStats();
foreach (var s in perThread) total.Merge(s);

string report = ReportWriter.Build(game, opt, total, sw.Elapsed);
Console.WriteLine();
Console.WriteLine(ReportWriter.Summary(game, total));
Directory.CreateDirectory(opt.OutDir);
string path = Path.Combine(opt.OutDir, $"rtp-{opt.Variant}-{ReportWriter.ShortCount(opt.Spins)}-seed{opt.Seed}.md");
File.WriteAllText(path, report);
Console.WriteLine($"Report written to {path} ({sw.Elapsed.TotalSeconds:0.0}s, {opt.Spins / sw.Elapsed.TotalSeconds:N0} spins/s)");
return 0;

static class CtsExtensions
{
    public static bool IsCancelled(this CancellationTokenSource cts) => cts.IsCancellationRequested;
}
