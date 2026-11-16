using KvevriGold.Engine.Config;
using KvevriGold.Engine.Features;
using KvevriGold.Engine.Game;
using KvevriGold.Engine.Math;

namespace KvevriGold.Tests;

/// <summary>Shared fixtures: the real Kvevri Gold symbols/paytable, plus hand-built reel sets.</summary>
internal static class TestGames
{
    public static GameConfig Real() => GameConfigLoader.LoadEmbedded();

    public static GameDefinition RealDefinition(string variant = "96") => new(Real(), variant);

    /// <summary>
    /// Test strips: each reel uses its own low symbol, so no way win can ever form (no symbol spans 3 reels).
    /// Stop 0 shows a scatter on the top row; stop 3 shows no scatter.
    /// </summary>
    public static readonly string[] Fillers = { "A", "K", "Q", "J", "T" };

    public static List<string> ScatterStrip(string filler) =>
        new() { "S", filler, filler, filler, filler, filler, filler, filler, filler, filler };

    /// <summary>A config whose base and free reel sets are the no-win scatter strips above.</summary>
    public static GameConfig ScatterConfig(List<List<string>>? freeSet = null)
    {
        var cfg = Real();
        var scatterSet = Fillers.Select(ScatterStrip).ToList();
        cfg.ReelSets = new Dictionary<string, List<List<string>>>
        {
            ["t_base"] = scatterSet,
            ["t_free"] = freeSet ?? scatterSet,
        };
        cfg.Variants = new List<VariantConfig>
        {
            new()
            {
                Id = "test",
                TargetRtp = 0,
                BaseReelSets = { new WeightedReelSetConfig { ReelSet = "t_base", Weight = 1 } },
                FreeReelSets = { new WeightedReelSetConfig { ReelSet = "t_free", Weight = 1 } },
            },
        };
        return cfg;
    }

    public static ReelSet Set(SymbolTable symbols, params string[][] strips) =>
        new("test", strips.Select(s => new ReelStrip(s.Select(c => symbols[c]).ToArray())).ToArray());

    public static WaysEvaluator Evaluator(GameDefinition def) => def.Evaluator;
}
