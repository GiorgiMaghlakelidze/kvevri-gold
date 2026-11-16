using KvevriGold.Engine.Features;
using KvevriGold.Engine.Game;
using KvevriGold.Engine.Model;

namespace KvevriGold.Tests;

public class CascadeTests
{
    private readonly GameDefinition _def = TestGames.RealDefinition();

    [Fact]
    public void Cascade_DropsSurvivorsAndRefillsFromStripAbove()
    {
        var board = Board.FromCodes(_def.Symbols,
            new[] { "TA", "A", "K" }, new[] { "Q", "Q", "Q" }, new[] { "J", "J", "J" }, new[] { "T", "T", "T" }, new[] { "A", "A", "A" });
        // Reel 1 strip: index 5 = GR (next to fall), index 4 = CH.
        var set = TestGames.Set(_def.Symbols,
            new[] { "Q", "Q", "Q", "Q", "CH", "GR" }, new[] { "K", "K", "K" }, new[] { "K", "K", "K" }, new[] { "K", "K", "K" }, new[] { "K", "K", "K" });
        board.SetRefill(set, 5, 0, 0, 0, 0);

        // Remove the middle and bottom cells of reel 1.
        board.Cascade((1UL << Board.CellIndex(0, 1, 3)) | (1UL << Board.CellIndex(0, 2, 3)));

        Assert.Equal(new[] { "CH", "GR", "TA" }, board.ToCodes(_def.Symbols)[0]);
        Assert.Equal(new[] { "Q", "Q", "Q" }, board.ToCodes(_def.Symbols)[1]); // untouched reel
    }

    [Fact]
    public void Spin_SetsRefillToPositionAboveTheWindow()
    {
        var set = TestGames.Set(_def.Symbols,
            new[] { "TA", "KN", "KV", "CH", "GR" }, new[] { "A", "K", "Q" }, new[] { "A", "K", "Q" }, new[] { "A", "K", "Q" }, new[] { "A", "K", "Q" });
        var board = new Board(5, 3);
        board.SetStops(set, 2, 0, 0, 0, 0); // reel 1 shows KV, CH, GR; TA... above is KN (index 1)
        board.Cascade(1UL << Board.CellIndex(0, 2, 3));
        Assert.Equal(new[] { "KN", "KV", "CH" }, board.ToCodes(_def.Symbols)[0]);
        board.Cascade(1UL << Board.CellIndex(0, 0, 3));
        Assert.Equal(new[] { "TA", "KV", "CH" }, board.ToCodes(_def.Symbols)[0]);
    }

    /// <summary>Board with a Tamada win whose refill creates a Grapes win, then nothing.</summary>
    private Board ChainBoard()
    {
        var board = Board.FromCodes(_def.Symbols,
            new[] { "TA", "A", "A" }, new[] { "TA", "K", "K" }, new[] { "TA", "Q", "Q" }, new[] { "J", "J", "J" }, new[] { "T", "T", "T" });
        var set = TestGames.Set(_def.Symbols,
            new[] { "A", "GR" }, new[] { "K", "GR" }, new[] { "Q", "GR" }, new[] { "J", "J" }, new[] { "T", "T" });
        board.SetRefill(set, 1, 1, 1, 1, 1);
        return board;
    }

    [Fact]
    public void Resolver_ChainsCascadesUntilNoWin()
    {
        var board = ChainBoard();
        var steps = new List<CascadeStep>();
        var outcome = _def.Cascades.Resolve(board, FlatMultiplier.One, steps);

        Assert.Equal(2, outcome.WinningSteps);
        Assert.Equal(20 + 6, outcome.Coins); // TA 3-kind then GR 3-kind
        Assert.Equal("TA", _def.Symbols.Code(steps[0].Wins.Single().Symbol));
        Assert.Equal("GR", _def.Symbols.Code(steps[1].Wins.Single().Symbol));
        Assert.Equal(new[] { "A", "A", "A" }, board.ToCodes(_def.Symbols)[0]);
    }

    [Fact]
    public void Resolver_AppliesProgressiveMultiplierPerWinningCascade()
    {
        var ctx = new GameContext { Multiplier = 1 };
        var outcome = _def.Cascades.Resolve(ChainBoard(), _def.FreeSpins.MultiplierFor(ctx), null);

        Assert.Equal(20 * 1 + 6 * 2, outcome.Coins);
        Assert.Equal(3, ctx.Multiplier); // raised after each of the 2 winning cascades
    }

    [Fact]
    public void ProgressiveMultiplier_IsCappedAtConfiguredMax()
    {
        var ctx = new GameContext { Multiplier = _def.Config.FreeSpins.MultiplierMax };
        var m = _def.FreeSpins.MultiplierFor(ctx);
        m.OnWinningStep();
        Assert.Equal(_def.Config.FreeSpins.MultiplierMax, ctx.Multiplier);
    }

    [Fact]
    public void WinningWilds_AreRemovedInCascade()
    {
        var board = Board.FromCodes(_def.Symbols,
            new[] { "TA", "A", "A" }, new[] { "W", "K", "K" }, new[] { "TA", "Q", "Q" }, new[] { "J", "J", "J" }, new[] { "T", "T", "T" });
        var set = TestGames.Set(_def.Symbols, new[] { "A" }, new[] { "K" }, new[] { "Q" }, new[] { "J" }, new[] { "T" });
        board.SetRefill(set, 0, 0, 0, 0, 0);
        _def.Cascades.Resolve(board, FlatMultiplier.One, null);
        Assert.Equal(0, board.CountAll(_def.Symbols.Wild));
    }
}
