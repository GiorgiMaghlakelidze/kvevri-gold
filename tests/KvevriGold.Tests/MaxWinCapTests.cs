using KvevriGold.Engine.Game;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Rng;

namespace KvevriGold.Tests;

public class MaxWinCapTests
{
    [Fact]
    public void RoundWin_IsCappedAndRemainingFreeSpinsAreForfeited()
    {
        // Free spins reel set is solid Tamada with wilds: every cascade wins 243 ways x 200 coins forever.
        var tamada = Enumerable.Range(0, 5).Select(r => Enumerable.Repeat(r == 0 ? "TA" : "W", 1).Concat(new[] { "TA", "TA" }).ToList()).ToList();
        var game = new ConfiguredSlotGame(TestGames.ScatterConfig(tamada), "test");
        var ctx = new GameContext();
        long cap = game.Definition.MaxWin.CapCoins;

        game.Spin(ctx, 1m, new ScriptedRng(0, 0, 0, 3, 3)); // trigger 8 free spins (40 coins scatter pay)
        var fs = game.Spin(ctx, 1m, new ScriptedRng(0));

        Assert.True(fs.MaxWinReached);
        Assert.Equal(cap, fs.RoundWinCoins);
        Assert.Equal(cap - 40, fs.WinCoins);
        Assert.Equal(0, fs.FreeSpinsRemaining);
        Assert.True(fs.RoundComplete);
        Assert.Equal(5000m, fs.RoundWin); // 5000x a 1.00 bet
        Assert.Equal(GameMode.Base, ctx.Mode);
    }

    [Fact]
    public void Cap_IsMaxWinXTimesBet()
    {
        var def = TestGames.RealDefinition();
        Assert.Equal(5000L * 20, def.MaxWin.CapCoins);
    }

    [Fact]
    public void Simulated_RoundWins_NeverExceedCap()
    {
        var game = GameFactory.Create("96");
        var ctx = new GameContext();
        var rng = new SeededRng(7);
        for (int i = 0; i < 200_000; i++)
            Assert.True(game.PlayRound(ctx, rng).TotalWinCoins <= game.Definition.MaxWin.CapCoins);
    }
}
