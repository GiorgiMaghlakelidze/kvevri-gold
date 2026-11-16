using KvevriGold.Engine.Game;

namespace KvevriGold.Api;

/// <summary>Public game rules sent to clients. Reel strips stay on the server.</summary>
public static class ConfigView
{
    public static object From(SlotGameBase game)
    {
        var def = game.Definition;
        var cfg = def.Config;
        double coins = cfg.CoinsPerBet;
        return new
        {
            cfg.Name,
            Variant = def.VariantId,
            TargetRtp = def.Variant.TargetRtp,
            AvailableVariants = cfg.Variants.Select(v => new { v.Id, v.TargetRtp }),
            cfg.Reels,
            cfg.Rows,
            Ways = (int)System.Math.Pow(cfg.Rows, cfg.Reels),
            cfg.CoinsPerBet,
            cfg.MaxWinX,
            Symbols = cfg.Symbols.Select(s => new { s.Code, s.Name, Kind = s.Kind.ToString() }),
            // Pays per way in multiples of the total bet, for 3/4/5 of a kind.
            Paytable = cfg.Paytable.ToDictionary(p => p.Key, p => p.Value.Select(v => v / coins).ToArray()),
            ScatterPays = cfg.Scatter.Pays.ToDictionary(p => p.Key, p => p.Value / coins),
            FreeSpinsAwards = cfg.Scatter.Awards,
            FreeSpinsRetriggers = cfg.Scatter.Retriggers,
            cfg.FreeSpins,
        };
    }
}
