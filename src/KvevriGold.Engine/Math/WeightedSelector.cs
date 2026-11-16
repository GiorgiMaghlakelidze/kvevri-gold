using KvevriGold.Engine.Rng;

namespace KvevriGold.Engine.Math;

/// <summary>Picks one item with probability proportional to its integer weight.</summary>
public sealed class WeightedSelector<T>
{
    private readonly T[] _items;
    private readonly int[] _cumulative;

    public WeightedSelector(IEnumerable<(T Item, int Weight)> entries)
    {
        var list = entries.ToList();
        if (list.Count == 0) throw new ArgumentException("At least one entry is required.", nameof(entries));
        _items = new T[list.Count];
        _cumulative = new int[list.Count];
        int total = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Weight <= 0) throw new ArgumentException("Weights must be positive.", nameof(entries));
            total = checked(total + list[i].Weight);
            _items[i] = list[i].Item;
            _cumulative[i] = total;
        }
        TotalWeight = total;
    }

    public int TotalWeight { get; }
    public int Count => _items.Length;

    public T Pick(IRng rng)
    {
        if (_items.Length == 1) return _items[0];
        int r = rng.NextInt(TotalWeight);
        int lo = 0, hi = _cumulative.Length - 1;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (r < _cumulative[mid]) hi = mid; else lo = mid + 1;
        }
        return _items[lo];
    }

    public IEnumerable<(T Item, int Weight)> Entries =>
        _items.Select((item, i) => (item, _cumulative[i] - (i == 0 ? 0 : _cumulative[i - 1])));
}
