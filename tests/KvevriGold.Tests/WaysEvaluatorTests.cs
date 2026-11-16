using KvevriGold.Engine.Game;
using KvevriGold.Engine.Math;
using KvevriGold.Engine.Model;

namespace KvevriGold.Tests;

public class WaysEvaluatorTests
{
    private readonly GameDefinition _def = TestGames.RealDefinition();

    private (long Coins, List<WayWin> Wins, ulong Mask) Eval(params string[][] reels)
    {
        var board = Board.FromCodes(_def.Symbols, reels);
        var wins = new List<WayWin>();
        long coins = _def.Evaluator.Evaluate(board, wins, out ulong mask);
        return (coins, wins, mask);
    }

    [Fact]
    public void ThreeOfAKind_SingleWay_PaysPaytableValue()
    {
        var (coins, wins, _) = Eval(
            new[] { "TA", "A", "K" },
            new[] { "Q", "TA", "J" },
            new[] { "J", "Q", "TA" },
            new[] { "A", "K", "Q" },
            new[] { "K", "J", "T" });

        var win = Assert.Single(wins);
        Assert.Equal("TA", _def.Symbols.Code(win.Symbol));
        Assert.Equal(3, win.Length);
        Assert.Equal(1, win.Ways);
        Assert.Equal(20, coins); // Tamada 3-of-a-kind = 20 coins = 1x bet
    }

    [Fact]
    public void Ways_AreProductOfMatchesPerReel()
    {
        // GR: 2 on reel 1, 1 on reel 2, 3 on reel 3, 2 on reel 4 -> 12 ways of 4-of-a-kind (15 coins each)
        var (coins, wins, _) = Eval(
            new[] { "GR", "GR", "A" },
            new[] { "K", "GR", "Q" },
            new[] { "GR", "GR", "GR" },
            new[] { "GR", "T", "GR" },
            new[] { "A", "K", "Q" });

        var win = Assert.Single(wins);
        Assert.Equal(4, win.Length);
        Assert.Equal(12, win.Ways);
        Assert.Equal(12 * 15, coins);
    }

    [Fact]
    public void FiveOfAKind_PaysTopValue()
    {
        var (coins, wins, _) = Eval(
            new[] { "KN", "A", "A" },
            new[] { "KN", "K", "K" },
            new[] { "KN", "Q", "Q" },
            new[] { "KN", "J", "J" },
            new[] { "KN", "T", "T" });

        Assert.Equal(150, coins);
        Assert.Equal(5, Assert.Single(wins).Length);
    }

    [Fact]
    public void TwoOfAKind_DoesNotPay()
    {
        var (coins, wins, mask) = Eval(
            new[] { "TA", "A", "A" },
            new[] { "TA", "K", "K" },
            new[] { "Q", "Q", "J" },
            new[] { "J", "J", "T" },
            new[] { "T", "T", "A" });

        Assert.Equal(0, coins);
        Assert.Empty(wins);
        Assert.Equal(0UL, mask);
    }

    [Fact]
    public void Ways_MustStartOnLeftmostReel()
    {
        var (coins, _, _) = Eval(
            new[] { "A", "A", "A" },
            new[] { "TA", "K", "K" },
            new[] { "TA", "Q", "Q" },
            new[] { "TA", "J", "J" },
            new[] { "TA", "T", "T" });
        Assert.Equal(0, coins);
    }

    [Fact]
    public void Wild_SubstitutesForPayingSymbols_AndCountsAsExtraWays()
    {
        // KV x1 on reel 1; reel 2 has KV + W (2 ways); reel 3 has W (1) -> 2 ways of 3-of-a-kind, 10 coins each
        var (coins, wins, _) = Eval(
            new[] { "KV", "A", "A" },
            new[] { "KV", "W", "K" },
            new[] { "Q", "W", "Q" },
            new[] { "J", "J", "J" },
            new[] { "T", "T", "T" });

        var win = Assert.Single(wins.Where(w => _def.Symbols.Code(w.Symbol) == "KV"));
        Assert.Equal(2, win.Ways);
        Assert.Equal(20, win.Coins);
        // The A on reel 1 also uses the wilds: A, W, W -> 1 way of 3 (4 coins)
        var ace = Assert.Single(wins.Where(w => _def.Symbols.Code(w.Symbol) == "A"));
        Assert.Equal(2, ace.Ways); // 2 A on reel 1 x 1 wild x 1 wild
        Assert.Equal(20 + 2 * 4, coins);
    }

    [Fact]
    public void Wild_OnFirstReel_DoesNotStartAWay()
    {
        var (coins, _, _) = Eval(
            new[] { "W", "W", "W" },
            new[] { "TA", "K", "K" },
            new[] { "TA", "Q", "Q" },
            new[] { "J", "J", "J" },
            new[] { "T", "T", "T" });
        Assert.Equal(0, coins);
    }

    [Fact]
    public void Wild_DoesNotSubstituteForScatter()
    {
        var (coins, _, _) = Eval(
            new[] { "S", "A", "K" },
            new[] { "W", "Q", "J" },
            new[] { "W", "J", "T" },
            new[] { "T", "T", "T" },
            new[] { "Q", "Q", "Q" });
        // Only A (A, W, W) and K (K, W, W) win; the scatter never forms ways.
        Assert.Equal(4 + 4, coins);
        Assert.True(_def.Wilds.Substitutes(_def.Symbols.Wild, _def.Symbols["TA"]));
        Assert.False(_def.Wilds.Substitutes(_def.Symbols.Wild, _def.Symbols.Scatter));
    }

    [Fact]
    public void WinMask_CoversExactlyTheWinningCells()
    {
        var (_, _, mask) = Eval(
            new[] { "TA", "A", "K" },
            new[] { "Q", "TA", "J" },
            new[] { "J", "Q", "TA" },
            new[] { "A", "K", "Q" },
            new[] { "K", "J", "T" });
        ulong expected = (1UL << Board.CellIndex(0, 0, 3)) | (1UL << Board.CellIndex(1, 1, 3)) | (1UL << Board.CellIndex(2, 2, 3));
        Assert.Equal(expected, mask);
    }

    [Fact]
    public void MultipleSymbols_PayIndependently()
    {
        var (coins, wins, _) = Eval(
            new[] { "TA", "CH", "A" },
            new[] { "TA", "CH", "K" },
            new[] { "TA", "CH", "Q" },
            new[] { "TA", "J", "J" },
            new[] { "T", "T", "T" });
        Assert.Equal(2, wins.Count);
        Assert.Equal(60 + 8, coins); // TA 4-of-a-kind + CH 3-of-a-kind
    }
}
