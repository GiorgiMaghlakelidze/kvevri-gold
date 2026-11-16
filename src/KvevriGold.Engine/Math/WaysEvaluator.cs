using KvevriGold.Engine.Features;
using KvevriGold.Engine.Model;

namespace KvevriGold.Engine.Math;

/// <summary>One winning symbol group in an all-ways evaluation.</summary>
public readonly record struct WayWin(byte Symbol, int Length, int Ways, int PayPerWay, long Coins, ulong Mask);

/// <summary>
/// "All ways" evaluator (243 ways on 5x3): a symbol pays when it appears on adjacent reels starting from
/// the leftmost reel, in any row. The number of ways is the product of matching cells per reel.
/// Only the longest run per symbol is paid.
/// </summary>
public sealed class WaysEvaluator
{
    private readonly Paytable _paytable;
    private readonly SymbolTable _symbols;
    private readonly WildRules _wilds;

    public WaysEvaluator(Paytable paytable, SymbolTable symbols, WildRules wilds)
    {
        _paytable = paytable;
        _symbols = symbols;
        _wilds = wilds;
    }

    /// <summary>
    /// Evaluates the board. Returns total coins (before any multiplier) and the mask of all cells that are
    /// part of a win. When <paramref name="wins"/> is not null each winning group is appended to it.
    /// </summary>
    public long Evaluate(Board board, List<WayWin>? wins, out ulong winMask)
    {
        winMask = 0;
        long total = 0;
        int rows = board.Rows;
        byte wild = _wilds.Wild;
        int firstWildReel = _wilds.FirstWildReel;
        foreach (byte symbol in _symbols.PayingSymbols)
        {
            int ways = 1, length = 0;
            ulong mask = 0;
            for (int reel = 0; reel < board.Reels; reel++)
            {
                int matches = 0;
                for (int row = 0; row < rows; row++)
                {
                    byte cell = board[reel, row];
                    if (cell == symbol || (cell == wild && reel >= firstWildReel))
                    {
                        matches++;
                        mask |= 1UL << (reel * rows + row);
                    }
                }
                if (matches == 0) break;
                ways *= matches;
                length++;
            }
            int pay = _paytable.WayPay(symbol, length);
            if (pay == 0) continue;
            // Mask built on the failing reel is always empty, so mask covers exactly the paying reels.
            long coins = (long)pay * ways;
            total += coins;
            winMask |= mask;
            wins?.Add(new WayWin(symbol, length, ways, pay, coins, mask));
        }
        return total;
    }
}
