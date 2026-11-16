using KvevriGold.Engine.Math;
using KvevriGold.Engine.Rng;

namespace KvevriGold.Engine.Model;

/// <summary>
/// The visible symbol window (reels x rows). Row 0 is the top row. The board remembers, per reel,
/// which strip position sits directly above the window so cascades refill from the real strip.
/// Cell index = reel * Rows + row (also used as the bit index in win masks).
/// </summary>
public sealed class Board
{
    private readonly byte[] _cells;
    private readonly int[] _nextIndex;
    private readonly byte[] _column;
    private ReelSet? _refill;

    public Board(int reels, int rows)
    {
        if (reels * rows > 64) throw new ArgumentException("Boards larger than 64 cells are not supported.");
        Reels = reels;
        Rows = rows;
        _cells = new byte[reels * rows];
        _nextIndex = new int[reels];
        _column = new byte[rows];
        Stops = new int[reels];
    }

    public int Reels { get; }
    public int Rows { get; }

    /// <summary>Stop positions of the last spin (strip index shown on the top row).</summary>
    public int[] Stops { get; }

    public string? ReelSetName => _refill?.Name;

    public byte this[int reel, int row]
    {
        get => _cells[reel * Rows + row];
        set => _cells[reel * Rows + row] = value;
    }

    public static int CellIndex(int reel, int row, int rows) => reel * rows + row;

    /// <summary>Spins every reel to a uniformly random stop on the given reel set.</summary>
    public void Spin(ReelSet reelSet, IRng rng)
    {
        if (reelSet.Reels != Reels) throw new ArgumentException("Reel set does not match board width.");
        _refill = reelSet;
        for (int reel = 0; reel < Reels; reel++)
        {
            ReelStrip strip = reelSet.Strips[reel];
            int stop = rng.NextInt(strip.Length);
            SetReel(reel, strip, stop);
        }
    }

    /// <summary>Places the board at explicit stops (used for replays and tests).</summary>
    public void SetStops(ReelSet reelSet, params int[] stops)
    {
        if (stops.Length != Reels) throw new ArgumentException("One stop per reel is required.");
        _refill = reelSet;
        for (int reel = 0; reel < Reels; reel++) SetReel(reel, reelSet.Strips[reel], stops[reel]);
    }

    private void SetReel(int reel, ReelStrip strip, int stop)
    {
        Stops[reel] = stop;
        int baseIdx = reel * Rows;
        for (int row = 0; row < Rows; row++) _cells[baseIdx + row] = strip[stop + row];
        _nextIndex[reel] = stop - 1;
    }

    /// <summary>
    /// Removes the cells in <paramref name="removeMask"/>, lets the remaining symbols fall down and fills
    /// the gaps from the strip positions above the window (continuing up the strip on later cascades).
    /// </summary>
    public void Cascade(ulong removeMask)
    {
        if (removeMask == 0) return;
        if (_refill is null) throw new InvalidOperationException("Board has no refill source; call Spin or SetStops first.");
        for (int reel = 0; reel < Reels; reel++)
        {
            int baseIdx = reel * Rows;
            ulong reelMask = (removeMask >> baseIdx) & ((1UL << Rows) - 1);
            if (reelMask == 0) continue;
            int write = Rows - 1;
            for (int row = Rows - 1; row >= 0; row--)
            {
                if ((reelMask & (1UL << row)) == 0) _column[write--] = _cells[baseIdx + row];
            }
            ReelStrip strip = _refill.Strips[reel];
            while (write >= 0)
            {
                _column[write--] = strip[_nextIndex[reel]];
                _nextIndex[reel]--;
            }
            for (int row = 0; row < Rows; row++) _cells[baseIdx + row] = _column[row];
        }
    }

    public int CountOnReel(int reel, byte symbol)
    {
        int n = 0, baseIdx = reel * Rows;
        for (int row = 0; row < Rows; row++) if (_cells[baseIdx + row] == symbol) n++;
        return n;
    }

    public int CountAll(byte symbol)
    {
        int n = 0;
        foreach (byte c in _cells) if (c == symbol) n++;
        return n;
    }

    /// <summary>Builds a board from symbol codes, one array per reel listed top to bottom.</summary>
    public static Board FromCodes(SymbolTable symbols, params string[][] reels)
    {
        var board = new Board(reels.Length, reels[0].Length);
        for (int r = 0; r < reels.Length; r++)
            for (int row = 0; row < board.Rows; row++)
                board[r, row] = symbols[reels[r][row]];
        return board;
    }

    /// <summary>Sets where cascade refills come from (tests and replays).</summary>
    public void SetRefill(ReelSet reelSet, params int[] nextIndex)
    {
        _refill = reelSet;
        Array.Copy(nextIndex, _nextIndex, Reels);
    }

    public string[][] ToCodes(SymbolTable symbols)
    {
        var result = new string[Reels][];
        for (int r = 0; r < Reels; r++)
        {
            result[r] = new string[Rows];
            for (int row = 0; row < Rows; row++) result[r][row] = symbols.Code(this[r, row]);
        }
        return result;
    }

    public override string ToString() => string.Join(" | ", Enumerable.Range(0, Reels)
        .Select(r => string.Join(",", Enumerable.Range(0, Rows).Select(row => _cells[r * Rows + row]))));
}
