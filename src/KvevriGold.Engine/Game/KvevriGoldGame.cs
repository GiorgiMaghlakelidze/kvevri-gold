using KvevriGold.Engine.Config;

namespace KvevriGold.Engine.Game;

/// <summary>Kvevri Gold: 5x3, 243 ways, cascades, Rtveli free spins with a progressive multiplier.</summary>
public abstract class KvevriGoldGame : SlotGameBase
{
    protected KvevriGoldGame(GameConfig config, string variantId) : base(new GameDefinition(config, variantId)) { }
}

/// <summary>~94% RTP variant (leaner base reel-set weighting).</summary>
public sealed class KvevriGold94 : KvevriGoldGame
{
    public const string Id = "94";
    public KvevriGold94(GameConfig? config = null) : base(config ?? GameConfigLoader.LoadEmbedded(), Id) { }
}

/// <summary>~96% RTP variant (richer base reel-set weighting).</summary>
public sealed class KvevriGold96 : KvevriGoldGame
{
    public const string Id = "96";
    public KvevriGold96(GameConfig? config = null) : base(config ?? GameConfigLoader.LoadEmbedded(), Id) { }
}

/// <summary>Creates game instances by variant id.</summary>
public static class GameFactory
{
    public static IReadOnlyList<string> Variants { get; } = new[] { KvevriGold94.Id, KvevriGold96.Id };

    public static KvevriGoldGame Create(string variant, GameConfig? config = null) => variant switch
    {
        KvevriGold94.Id => new KvevriGold94(config),
        KvevriGold96.Id => new KvevriGold96(config),
        _ => throw new ArgumentException($"Unknown variant '{variant}'. Use 94 or 96.", nameof(variant)),
    };
}

/// <summary>Generic game over any config/variant (used by tests with hand-built configs).</summary>
public sealed class ConfiguredSlotGame : SlotGameBase
{
    public ConfiguredSlotGame(GameConfig config, string variantId) : base(new GameDefinition(config, variantId)) { }
}
