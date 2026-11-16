using KvevriGold.Engine.Config;
using KvevriGold.Engine.Features;
using KvevriGold.Engine.Math;

namespace KvevriGold.Engine.Game;

/// <summary>
/// A validated, compiled game for one RTP variant: symbol table, paytable, reel sets with their weights
/// and the feature modules. Immutable and thread-safe; share one instance between sessions/threads.
/// </summary>
public sealed class GameDefinition
{
    public GameDefinition(GameConfig config, string variantId)
    {
        Config = config;
        Variant = config.Variants.FirstOrDefault(v => v.Id == variantId)
            ?? throw new ArgumentException($"Unknown variant '{variantId}'. Known: {string.Join(", ", config.Variants.Select(v => v.Id))}.");
        if (config.Reels < 3 || config.Rows < 1) throw new InvalidDataException("Board must be at least 3 reels by 1 row.");

        Symbols = new SymbolTable(config.Symbols);
        Paytable = new Paytable(config, Symbols);
        Wilds = new WildRules(Symbols);

        var compiled = new Dictionary<string, ReelSet>();
        ReelSet Compile(string name)
        {
            if (compiled.TryGetValue(name, out var existing)) return existing;
            if (!config.ReelSets.TryGetValue(name, out var strips)) throw new InvalidDataException($"Unknown reel set '{name}'.");
            if (strips.Count != config.Reels) throw new InvalidDataException($"Reel set '{name}' must have {config.Reels} strips.");
            var set = new ReelSet(name, strips.Select(s =>
            {
                if (s.Count < config.Rows) throw new InvalidDataException($"Reel set '{name}' has a strip shorter than the window.");
                return new ReelStrip(s.Select(code => Symbols[code]).ToArray());
            }).ToArray());
            Wilds.Validate(set);
            compiled[name] = set;
            return set;
        }

        BaseReelSets = new WeightedSelector<ReelSet>(Variant.BaseReelSets.Select(w => (Compile(w.ReelSet), w.Weight)));
        FreeReelSets = new WeightedSelector<ReelSet>(Variant.FreeReelSets.Select(w => (Compile(w.ReelSet), w.Weight)));

        Evaluator = new WaysEvaluator(Paytable, Symbols, Wilds);
        Cascades = new CascadeResolver(Evaluator, Symbols);
        FreeSpins = new FreeSpinsFeature(Paytable, config.FreeSpins);
        MaxWin = new MaxWinCap((long)config.MaxWinX * config.CoinsPerBet);
    }

    public GameConfig Config { get; }
    public VariantConfig Variant { get; }
    public string VariantId => Variant.Id;
    public int Reels => Config.Reels;
    public int Rows => Config.Rows;
    public int CoinsPerBet => Config.CoinsPerBet;

    public SymbolTable Symbols { get; }
    public Paytable Paytable { get; }
    public WildRules Wilds { get; }
    public WeightedSelector<ReelSet> BaseReelSets { get; }
    public WeightedSelector<ReelSet> FreeReelSets { get; }
    public WaysEvaluator Evaluator { get; }
    public CascadeResolver Cascades { get; }
    public FreeSpinsFeature FreeSpins { get; }
    public MaxWinCap MaxWin { get; }
}
