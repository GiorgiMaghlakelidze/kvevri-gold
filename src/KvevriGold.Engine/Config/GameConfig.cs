using System.Text.Json;
using System.Text.Json.Serialization;

namespace KvevriGold.Engine.Config;

/// <summary>Raw JSON game configuration: symbols, paytable, feature rules, reel sets and RTP variants.</summary>
public sealed class GameConfig
{
    public string Name { get; set; } = "";
    public int Reels { get; set; } = 5;
    public int Rows { get; set; } = 3;

    /// <summary>Paytable values are in coins; one total bet equals this many coins.</summary>
    public int CoinsPerBet { get; set; } = 20;

    /// <summary>Maximum win per round (base spin plus any free spins), in multiples of the bet.</summary>
    public int MaxWinX { get; set; } = 5000;

    public List<SymbolConfig> Symbols { get; set; } = new();

    /// <summary>Way pays per symbol code. Index 0 is 3-of-a-kind, index 1 is 4-of-a-kind, and so on.</summary>
    public Dictionary<string, int[]> Paytable { get; set; } = new();

    public ScatterConfig Scatter { get; set; } = new();
    public FreeSpinsConfig FreeSpins { get; set; } = new();

    /// <summary>Named reel sets. Each strip is listed top to bottom; one strip per reel.</summary>
    public Dictionary<string, List<List<string>>> ReelSets { get; set; } = new();

    public List<VariantConfig> Variants { get; set; } = new();

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true,
    };
}

public enum SymbolKind
{
    High,
    Low,
    Wild,
    Scatter,
}

public sealed class SymbolConfig
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public SymbolKind Kind { get; set; }
}

public sealed class ScatterConfig
{
    /// <summary>Scatter pays in coins (not multiplied by the free-spins multiplier), keyed by scatter count.</summary>
    public Dictionary<int, int> Pays { get; set; } = new();

    /// <summary>Free spins awarded in the base game, keyed by scatter count.</summary>
    public Dictionary<int, int> Awards { get; set; } = new();

    /// <summary>Extra free spins awarded during free spins, keyed by scatter count.</summary>
    public Dictionary<int, int> Retriggers { get; set; } = new();
}

public sealed class FreeSpinsConfig
{
    public int MultiplierStart { get; set; } = 1;
    public int MultiplierStep { get; set; } = 1;
    public int MultiplierMax { get; set; } = 10;
}

public sealed class VariantConfig
{
    public string Id { get; set; } = "";
    public double TargetRtp { get; set; }
    public List<WeightedReelSetConfig> BaseReelSets { get; set; } = new();
    public List<WeightedReelSetConfig> FreeReelSets { get; set; } = new();
}

public sealed class WeightedReelSetConfig
{
    public string ReelSet { get; set; } = "";
    public int Weight { get; set; } = 1;
}
