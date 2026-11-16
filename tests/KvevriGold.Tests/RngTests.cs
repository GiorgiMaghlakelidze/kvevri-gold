using KvevriGold.Engine.Game;
using KvevriGold.Engine.Math;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Rng;

namespace KvevriGold.Tests;

public class RngTests
{
    [Fact]
    public void SeededRng_SameSeed_SameSequence()
    {
        var a = new SeededRng(42);
        var b = new SeededRng(42);
        for (int i = 0; i < 10_000; i++) Assert.Equal(a.NextInt(1000), b.NextInt(1000));
    }

    [Fact]
    public void SeededRng_DifferentSeedsAndStreams_Differ()
    {
        var seqA = Draw(new SeededRng(1));
        Assert.NotEqual(seqA, Draw(new SeededRng(2)));
        Assert.NotEqual(Draw(SeededRng.ForStream(1, 0)), Draw(SeededRng.ForStream(1, 1)));
        Assert.Equal(Draw(SeededRng.ForStream(9, 3)), Draw(SeededRng.ForStream(9, 3)));
    }

    private static int[] Draw(IRng rng) => Enumerable.Range(0, 64).Select(_ => rng.NextInt(1 << 30)).ToArray();

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(65)]
    [InlineData(int.MaxValue)]
    public void Rngs_StayInRange(int max)
    {
        IRng[] rngs = { new SeededRng(5), new CryptoRng() };
        foreach (var rng in rngs)
            for (int i = 0; i < 2000; i++)
            {
                int v = rng.NextInt(max);
                Assert.InRange(v, 0, max - 1);
            }
    }

    [Fact]
    public void SeededRng_IsRoughlyUniform()
    {
        var rng = new SeededRng(123);
        var counts = new int[10];
        const int n = 1_000_000;
        for (int i = 0; i < n; i++) counts[rng.NextInt(10)]++;
        double chi2 = counts.Sum(c => System.Math.Pow(c - n / 10.0, 2) / (n / 10.0));
        Assert.True(chi2 < 27.9, $"chi-square {chi2:0.00} too high (df=9, p=0.001 critical 27.88)");
    }

    [Fact]
    public void Game_WithSameSeed_IsFullyDeterministic()
    {
        var game = GameFactory.Create("94");
        long[] Run()
        {
            var rng = new SeededRng(2026);
            var ctx = new GameContext();
            return Enumerable.Range(0, 5000).Select(_ => game.PlayRound(ctx, rng).TotalWinCoins).ToArray();
        }
        Assert.Equal(Run(), Run());
    }

    [Fact]
    public void ScriptedRng_ReplaysValues()
    {
        var rng = new ScriptedRng(4, 7);
        Assert.Equal(4, rng.NextInt(10));
        Assert.Equal(1, rng.NextInt(3)); // 7 mod 3
        Assert.Equal(4, rng.NextInt(10));
    }

    [Fact]
    public void WeightedSelector_FollowsWeights()
    {
        var sel = new WeightedSelector<string>(new[] { ("a", 1), ("b", 3) });
        var rng = new SeededRng(99);
        int b = Enumerable.Range(0, 100_000).Count(_ => sel.Pick(rng) == "b");
        Assert.InRange(b / 100_000.0, 0.74, 0.76);
        Assert.Throws<ArgumentException>(() => new WeightedSelector<string>(new[] { ("a", 0) }));
    }
}
