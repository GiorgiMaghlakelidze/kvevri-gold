#!/usr/bin/env python3
"""Generates src/KvevriGold.Engine/Config/kvevri-gold.json.

Reel strips are built from per-reel symbol counts (the tuning knobs below) and shuffled with a fixed
seed, so the output is reproducible. Constraints: scatters are spaced at least SCATTER_GAP positions
apart on a strip, and wilds never appear on reel 1.

Tuning workflow: change counts or reel-set weights here, regenerate, then measure with
  dotnet run -c Release --project src/KvevriGold.Simulator -- --variant 96 --spins 10000000
"""
import json, random, pathlib

SEED = 20251
SCATTER_GAP = 4

SYMBOLS = [
    ("TA", "Tamada", "High"), ("KN", "Kantsi", "High"), ("KV", "Kvevri", "High"),
    ("CH", "Churchkhela", "High"), ("GR", "Grapes", "High"),
    ("A", "Ace", "Low"), ("K", "King", "Low"), ("Q", "Queen", "Low"), ("J", "Jack", "Low"), ("T", "Ten", "Low"),
    ("W", "Golden Fleece Wild", "Wild"), ("S", "Rtveli Basket Scatter", "Scatter"),
]

# Coins per way for 3/4/5 of a kind; one bet = 20 coins.
PAYTABLE = {
    "TA": [20, 60, 200], "KN": [15, 40, 150], "KV": [10, 30, 100], "CH": [8, 20, 80], "GR": [6, 15, 60],
    "A": [4, 10, 30], "K": [4, 10, 30], "Q": [3, 8, 25], "J": [2, 6, 20], "T": [2, 5, 20],
}

# Per-reel symbol counts: order matches COLS.
COLS = ["TA", "KN", "KV", "CH", "GR", "A", "K", "Q", "J", "T", "W", "S"]
REEL_SETS = {
    # Richer base set.
    "base_harvest": [
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 0, 1],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 4, 2],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 4, 1],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 4, 2],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 3, 1],
    ],
    # Leaner base set.
    "base_cellar": [
        [2, 3, 3, 4, 5, 6, 6, 7, 7, 7, 0, 1],
        [2, 3, 3, 4, 5, 6, 6, 7, 7, 7, 3, 2],
        [2, 3, 3, 4, 5, 6, 6, 7, 7, 7, 3, 1],
        [2, 3, 3, 4, 5, 6, 6, 7, 7, 7, 3, 2],
        [2, 3, 3, 4, 5, 6, 6, 7, 7, 7, 2, 1],
    ],
    # Free spins set.
    "free_rtveli": [
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 0, 1],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 5, 2],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 5, 1],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 5, 2],
        [3, 3, 4, 4, 5, 5, 5, 6, 6, 6, 4, 1],
    ],
}

VARIANTS = [
    {"id": "94", "targetRtp": 0.94,
     "baseReelSets": [{"reelSet": "base_harvest", "weight": 583}, {"reelSet": "base_cellar", "weight": 417}],
     "freeReelSets": [{"reelSet": "free_rtveli", "weight": 1}]},
    {"id": "96", "targetRtp": 0.96,
     "baseReelSets": [{"reelSet": "base_harvest", "weight": 644}, {"reelSet": "base_cellar", "weight": 356}],
     "freeReelSets": [{"reelSet": "free_rtveli", "weight": 1}]},
]


def build_strip(counts, rng):
    pool = [sym for sym, n in zip(COLS, counts) for _ in range(n)]
    for _ in range(100000):
        rng.shuffle(pool)
        idx = [i for i, s in enumerate(pool) if s == "S"]
        n = len(pool)
        ok = all(min(abs(a - b), n - abs(a - b)) >= SCATTER_GAP for i, a in enumerate(idx) for b in idx[i + 1:])
        # avoid identical adjacent high symbols so 3-high stacks are rare
        ok = ok and all(not (pool[i] == pool[(i + 1) % n] and pool[i] in ("TA", "KN", "W")) for i in range(n))
        if ok:
            return list(pool)
    raise RuntimeError("could not satisfy strip constraints")


def main():
    rng = random.Random(SEED)
    config = {
        "name": "Kvevri Gold", "reels": 5, "rows": 3, "coinsPerBet": 20, "maxWinX": 5000,
        "symbols": [{"code": c, "name": n, "kind": k} for c, n, k in SYMBOLS],
        "paytable": PAYTABLE,
        "scatter": {"pays": {"3": 40, "4": 200, "5": 1000},
                    "awards": {"3": 8, "4": 12, "5": 15},
                    "retriggers": {"3": 5, "4": 8, "5": 10}},
        "freeSpins": {"multiplierStart": 1, "multiplierStep": 1, "multiplierMax": 10},
        "reelSets": {name: [build_strip(c, rng) for c in reels] for name, reels in REEL_SETS.items()},
        "variants": VARIANTS,
    }
    out = pathlib.Path(__file__).resolve().parent.parent / "src/KvevriGold.Engine/Config/kvevri-gold.json"
    text = json.dumps(config, indent=2)
    # keep strips on one line each for readability
    import re
    text = re.sub(r'\[\s+("[A-Z]{1,2}"(?:,\s+"[A-Z]{1,2}")*)\s+\]',
                  lambda m: "[" + re.sub(r",\s+", ", ", m.group(1)) + "]", text)
    out.write_text(text + "\n")
    for name, reels in REEL_SETS.items():
        print(name, [sum(c) for c in reels])


if __name__ == "__main__":
    main()
