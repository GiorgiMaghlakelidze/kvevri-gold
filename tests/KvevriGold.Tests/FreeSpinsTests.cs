using KvevriGold.Engine.Game;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Rng;

namespace KvevriGold.Tests;

public class FreeSpinsTests
{
    // Stop 0 = scatter visible on that reel, stop 3 = no scatter (see TestGames.ScatterStrip).
    private static ScriptedRng Stops(params int[] stops) => new(stops);

    private static int[] Scatters(int count) => Enumerable.Range(0, 5).Select(r => r < count ? 0 : 3).ToArray();

    [Theory]
    [InlineData(3, 8, 40)]
    [InlineData(4, 12, 200)]
    [InlineData(5, 15, 1000)]
    public void ScattersInBaseGame_TriggerFreeSpinsAndPay(int scatters, int expectedSpins, long expectedPay)
    {
        var game = new ConfiguredSlotGame(TestGames.ScatterConfig(), "test");
        var ctx = new GameContext();

        var result = game.Spin(ctx, 1m, Stops(Scatters(scatters)));

        Assert.Equal(GameMode.Base, result.Mode);
        Assert.Equal(scatters, result.Scatters);
        Assert.Equal(expectedSpins, result.FreeSpinsAwarded);
        Assert.Equal(expectedSpins, ctx.FreeSpinsRemaining);
        Assert.Equal(expectedPay, result.WinCoins);
        Assert.Equal(GameMode.FreeSpins, ctx.Mode);
        Assert.False(result.RoundComplete);
    }

    [Fact]
    public void TwoScatters_DoNotTrigger()
    {
        var game = new ConfiguredSlotGame(TestGames.ScatterConfig(), "test");
        var ctx = new GameContext();
        var result = game.Spin(ctx, 1m, Stops(Scatters(2)));
        Assert.Equal(0, result.FreeSpinsAwarded);
        Assert.Equal(0, result.WinCoins);
        Assert.True(result.RoundComplete);
        Assert.Equal(GameMode.Base, ctx.Mode);
    }

    [Fact]
    public void FreeSpins_AreNotCharged_AndCountDownToRoundEnd()
    {
        var game = new ConfiguredSlotGame(TestGames.ScatterConfig(), "test");
        var ctx = new GameContext();
        game.Spin(ctx, 2m, Stops(Scatters(3)));
        long roundId = ctx.RoundId;

        var rng = Stops(Scatters(0));
        for (int i = 1; i <= 8; i++)
        {
            var fs = game.Spin(ctx, 999m, rng); // bet ignored during free spins
            Assert.Equal(GameMode.FreeSpins, fs.Mode);
            Assert.Equal(0m, fs.Stake);
            Assert.Equal(2m, fs.Bet);
            Assert.Equal(roundId, fs.RoundId);
            Assert.Equal(8 - i, fs.FreeSpinsRemaining);
            Assert.Equal(i == 8, fs.RoundComplete);
        }
        Assert.Equal(GameMode.Base, ctx.Mode);
        var next = game.Spin(ctx, 1m, Stops(Scatters(3))); // next round is a paid base spin again
        Assert.Equal(GameMode.Base, next.Mode);
        Assert.Equal(1m, next.Stake);
        Assert.Equal(2m, next.Win); // 3 scatters = 40 coins = 2x bet
    }

    [Fact]
    public void ScattersDuringFreeSpins_Retrigger()
    {
        var game = new ConfiguredSlotGame(TestGames.ScatterConfig(), "test");
        var ctx = new GameContext();
        game.Spin(ctx, 1m, Stops(Scatters(3)));      // 8 free spins
        var fs = game.Spin(ctx, 1m, Stops(Scatters(3)));  // first free spin retriggers +5

        Assert.Equal(GameMode.FreeSpins, fs.Mode);
        Assert.Equal(5, fs.FreeSpinsAwarded);
        Assert.Equal(8 - 1 + 5, fs.FreeSpinsRemaining);
        Assert.Equal(13, ctx.FreeSpinsAwardedTotal);
        Assert.Equal(40, fs.WinCoins); // scatter pay still applies in free spins

        var outcome = PlayOut(game, ctx);
        Assert.Equal(13, outcome);
    }

    private static int PlayOut(SlotGameBase game, GameContext ctx)
    {
        var rng = Stops(Scatters(0));
        while (ctx.FreeSpinsRemaining > 0) game.Spin(ctx, 1m, rng);
        return ctx.FreeSpinsPlayed;
    }

    [Fact]
    public void Multiplier_ResetsAtStartOfEachFeature()
    {
        var game = new ConfiguredSlotGame(TestGames.ScatterConfig(), "test");
        var ctx = new GameContext { Multiplier = 7 };
        game.Spin(ctx, 1m, Stops(Scatters(3)));
        Assert.Equal(1, ctx.Multiplier);
    }
}
