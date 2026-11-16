using KvevriGold.Engine.Game;
using KvevriGold.Engine.Model;
using KvevriGold.Engine.Rng;
using Xunit.Abstractions;

namespace KvevriGold.Tests;

/// <summary>
/// Statistical check: 1,000,000 rounds per variant (single thread, fixed seed) must land within
/// 4 standard errors of the target RTP. The tight ±0.3 pp tuning is verified by the simulator at 10M+ rounds.
/// </summary>
public class RtpStatisticalTests
{
    private readonly ITestOutputHelper _out;
    public RtpStatisticalTests(ITestOutputHelper output) => _out = output;

    [Theory]
    [InlineData("94")]
    [InlineData("96")]
    [Trait("Category", "Statistical")]
    public void OneMillionRounds_RtpIsNearTarget(string variant)
    {
        const int rounds = 1_000_000;
        var game = GameFactory.Create(variant);
        var rng = new SeededRng(31337);
        var ctx = new GameContext();
        double bet = game.Definition.CoinsPerBet;
        double sum = 0, sumSq = 0;
        for (int i = 0; i < rounds; i++)
        {
            double x = game.PlayRound(ctx, rng).TotalWinCoins / bet;
            sum += x;
            sumSq += x * x;
        }
        double rtp = sum / rounds;
        double sd = System.Math.Sqrt(sumSq / rounds - rtp * rtp);
        double se = sd / System.Math.Sqrt(rounds);
        double target = game.Definition.Variant.TargetRtp;
        _out.WriteLine($"variant {variant}: RTP {rtp:P3} (target {target:P1}), sd {sd:0.000}, SE {se * 100:0.000} pp");
        Assert.InRange(rtp, target - 4 * se, target + 4 * se);
    }
}
