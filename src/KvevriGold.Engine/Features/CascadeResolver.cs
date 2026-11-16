using KvevriGold.Engine.Math;
using KvevriGold.Engine.Model;

namespace KvevriGold.Engine.Features;

/// <summary>Supplies the win multiplier for each cascade step and reacts to winning steps.</summary>
public interface ICascadeMultiplier
{
    int Current { get; }
    void OnWinningStep();
}

/// <summary>Base game: every cascade pays x1.</summary>
public sealed class FlatMultiplier : ICascadeMultiplier
{
    public static readonly FlatMultiplier One = new();
    public int Current => 1;
    public void OnWinningStep() { }
}

/// <summary>One evaluated cascade step: the board before removal, its wins and the multiplier applied.</summary>
public sealed record CascadeStep(string[][]? Board, IReadOnlyList<WayWin> Wins, int Multiplier, long Coins);

public readonly record struct CascadeOutcome(long Coins, int WinningSteps);

/// <summary>
/// Avalanche / cascade loop: evaluate ways, pay (times the current multiplier), remove the winning
/// symbols, drop and refill, and repeat until a board has no win.
/// </summary>
public sealed class CascadeResolver
{
    private readonly WaysEvaluator _evaluator;
    private readonly SymbolTable _symbols;
    private readonly int _maxSteps;

    public CascadeResolver(WaysEvaluator evaluator, SymbolTable symbols, int maxSteps = 1000)
    {
        _evaluator = evaluator;
        _symbols = symbols;
        _maxSteps = maxSteps;
    }

    /// <param name="board">Board to resolve; it is mutated by each cascade.</param>
    /// <param name="multiplier">Multiplier source for each winning step.</param>
    /// <param name="steps">Optional sink for step details (null on the fast path).</param>
    /// <param name="stopAtCoins">Stops cascading once this much has been won (the remaining max-win room).</param>
    public CascadeOutcome Resolve(Board board, ICascadeMultiplier multiplier, List<CascadeStep>? steps, long stopAtCoins = long.MaxValue)
    {
        long total = 0;
        int winningSteps = 0;
        List<WayWin>? wins = steps is null ? null : new List<WayWin>();
        while (winningSteps < _maxSteps)
        {
            wins?.Clear();
            long coins = _evaluator.Evaluate(board, wins, out ulong mask);
            if (coins == 0) break;
            int mult = multiplier.Current;
            long paid = coins * mult;
            total += paid;
            winningSteps++;
            steps?.Add(new CascadeStep(board.ToCodes(_symbols), wins!.ToArray(), mult, paid));
            multiplier.OnWinningStep();
            board.Cascade(mask);
            if (total >= stopAtCoins) break;
        }
        return new CascadeOutcome(total, winningSteps);
    }
}
