namespace KvevriGold.Web.Game;

/// <summary>Display names for the symbol codes used by the engine config.</summary>
public static class SymbolInfo
{
    public static string Name(string code) => code switch
    {
        "TA" => "Tamada",
        "KN" => "Kantsi",
        "KV" => "Kvevri",
        "CH" => "Churchkhela",
        "GR" => "Grapes",
        "W" => "Golden Fleece (wild)",
        "S" => "Rtveli basket (scatter)",
        "T" => "10",
        _ => code,
    };

    public static string Rank(string code) => code == "T" ? "10" : code;
}
