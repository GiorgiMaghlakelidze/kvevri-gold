using KvevriGold.Engine.Config;
using KvevriGold.Engine.Game;

namespace KvevriGold.Tests;

public class ConfigTests
{
    [Fact]
    public void EmbeddedConfig_LoadsBothVariants()
    {
        var cfg = GameConfigLoader.LoadEmbedded();
        Assert.Equal("Kvevri Gold", cfg.Name);
        Assert.Equal(new[] { "94", "96" }, cfg.Variants.Select(v => v.Id).OrderBy(x => x));
        foreach (var v in GameFactory.Variants)
        {
            var game = GameFactory.Create(v, cfg);
            Assert.Equal(v, game.VariantId);
            Assert.Equal(0.01 * int.Parse(v), game.Definition.Variant.TargetRtp, 6);
        }
    }

    [Fact]
    public void Variants_UseDifferentBaseReelSetWeights()
    {
        var a = GameFactory.Create("94").Definition.BaseReelSets.Entries.Select(e => (e.Item.Name, e.Weight)).ToArray();
        var b = GameFactory.Create("96").Definition.BaseReelSets.Entries.Select(e => (e.Item.Name, e.Weight)).ToArray();
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Strips_HaveNoWildsOnFirstReel()
    {
        var cfg = GameConfigLoader.LoadEmbedded();
        foreach (var (name, strips) in cfg.ReelSets)
            Assert.DoesNotContain("W", strips[0]);
    }

    [Fact]
    public void WildOnFirstReel_IsRejected()
    {
        var cfg = TestGames.ScatterConfig();
        cfg.ReelSets["t_base"][0][1] = "W";
        Assert.Throws<InvalidDataException>(() => new GameDefinition(cfg, "test"));
    }

    [Fact]
    public void UnknownVariant_Throws()
    {
        Assert.Throws<ArgumentException>(() => GameFactory.Create("99"));
    }

    [Fact]
    public void Paytable_IsMonotonicByLength()
    {
        var cfg = GameConfigLoader.LoadEmbedded();
        foreach (var (code, pays) in cfg.Paytable)
            for (int i = 1; i < pays.Length; i++) Assert.True(pays[i] > pays[i - 1], code);
    }
}
