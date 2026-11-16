using KvevriGold.Engine.Math;

namespace KvevriGold.Engine.Features;

/// <summary>
/// Wild handling: the Golden Kvevri wild substitutes for every paying symbol (never for the scatter),
/// has no way pay of its own and only appears on reels 2-5, so every way is anchored by a real
/// symbol on reel 1. Wilds that are part of a winning way are removed in the cascade like any other symbol.
/// </summary>
public sealed class WildRules
{
    public WildRules(SymbolTable symbols, int firstWildReel = 1)
    {
        Wild = symbols.Wild;
        Scatter = symbols.Scatter;
        FirstWildReel = firstWildReel;
    }

    public byte Wild { get; }
    public byte Scatter { get; }

    /// <summary>Zero-based index of the first reel where wilds may appear.</summary>
    public int FirstWildReel { get; }

    public bool Substitutes(byte boardSymbol, byte target) =>
        boardSymbol == target || (boardSymbol == Wild && target != Scatter && target != Wild);

    /// <summary>Throws if a reel set places wilds on reels where they are not allowed.</summary>
    public void Validate(ReelSet reelSet)
    {
        for (int reel = 0; reel < FirstWildReel && reel < reelSet.Reels; reel++)
            if (reelSet.Strips[reel].CountOf(Wild) > 0)
                throw new InvalidDataException($"Reel set '{reelSet.Name}' has wilds on reel {reel + 1}.");
    }
}
