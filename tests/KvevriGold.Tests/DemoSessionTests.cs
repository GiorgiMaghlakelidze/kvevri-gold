using KvevriGold.Engine.Game;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Rng;
using KvevriGold.Web.Game;

namespace KvevriGold.Tests;

/// <summary>Play-money accounting of the browser demo (src/KvevriGold.Web/Game/DemoSession.cs).</summary>
public class DemoSessionTests
{
    [Fact]
    public void BalanceAndStats_MatchTheEngineResults()
    {
        var session = new DemoSession(new SeededRng(7));
        decimal expected = DemoSession.StartingBalance, stakes = 0, wins = 0;
        for (int i = 0; i < 2000 && session.CanAfford; i++)
        {
            var r = session.Spin();
            expected += r.Win - r.Stake;
            stakes += r.Stake;
            wins += r.Win;
        }
        Assert.Equal(expected, session.Balance);
        Assert.Equal(stakes, session.TotalBet);
        Assert.Equal(wins, session.TotalWon);
        Assert.Equal(stakes, session.PaidSpins * session.Bet);
    }

    [Fact]
    public void SeededSession_ReplaysTheSameSpinsAsTheEngine()
    {
        var session = new DemoSession();
        Assert.True(session.TryUseSeed(1155));
        var game = GameFactory.Create("96");
        var ctx = new GameContext();
        var rng = new SeededRng(1155);
        for (int i = 0; i < 50; i++)
        {
            var expected = game.Spin(ctx, 1m, rng);
            var actual = session.Spin();
            Assert.Equal(expected.Stops, actual.Stops);
            Assert.Equal(expected.Win, actual.Win);
            Assert.Equal(expected.Mode, actual.Mode);
        }
        Assert.False(session.TryUseSeed(1)); // only before the first spin
    }

    [Fact]
    public void FreeSpins_AreNotCharged_AndLockBetAndVariant()
    {
        var session = new DemoSession(new SeededRng(1155)); // seed 1155 triggers free spins on the first spin at bet 1, RTP 96
        var first = session.Spin();
        Assert.True(first.FreeSpinsAwarded > 0);
        Assert.True(session.InFreeSpins);

        session.SetBet(5m);
        session.SetVariant("94");
        Assert.Equal(1m, session.Bet);
        Assert.Equal("96", session.Variant);

        decimal before = session.TotalBet;
        while (session.InFreeSpins)
        {
            var fs = session.Spin();
            Assert.Equal(GameMode.FreeSpins, fs.Mode);
            Assert.Equal(0m, fs.Stake);
        }
        Assert.Equal(before, session.TotalBet);
        Assert.Equal(8, session.FreeSpinsPlayed);
    }

    [Fact]
    public void Spin_RefusesWhenBalanceIsTooLow_AndRefillRestoresIt()
    {
        var session = new DemoSession(new SeededRng(3));
        session.SetBet(20m);
        while (session.CanAfford) session.Spin();
        Assert.Throws<InvalidOperationException>(() => session.Spin());
        session.RefillBalance();
        Assert.Equal(DemoSession.StartingBalance, session.Balance);
    }

    [Fact]
    public void SetBet_RejectsValuesOutsideTheBetLadder()
    {
        var session = new DemoSession(new SeededRng(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => session.SetBet(3.33m));
        Assert.Throws<ArgumentOutOfRangeException>(() => session.SetVariant("99"));
    }
}
