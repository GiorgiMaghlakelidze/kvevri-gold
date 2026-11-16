# Kvevri Gold RTP simulation: variant 96

Generated 2026-11-16 20:45 +04:00 by `KvevriGold.Simulator`.

| Setting | Value |
|---|---|
| Paid rounds | 10,000,000 |
| Seed | 20251116 (crypto RNG, 8 threads) |
| Base reel sets | `base_harvest` w=644, `base_cellar` w=356 |
| Free spins reel sets | `free_rtveli` w=1 |
| Runtime | 8.2s (1,220,421 rounds/s) |

## Results

| Metric | Value |
|---|---|
| **RTP** | **95.975%** |
| Target RTP | 96.000% (within ±0.3 pp, diff -0.025 pp) |
| 95% confidence interval | 95.646% to 96.304% (±0.329 pp) |
| Base game RTP contribution | 71.503% |
| Free spins RTP contribution | 24.472% |
| Hit frequency (round wins > 0) | 40.866% (1 in 2.4) |
| Free spins trigger frequency | 1 in 135.6 (0.737%) |
| Average free spins win | 33.19x bet |
| Average free spins played per trigger | 8.50 (retriggers: 4,365) |
| Standard deviation per round | 5.310x bet |
| Max win observed | 1,037.35x bet |
| Max win cap (5,000x) hits | 0 |

## RTP by base reel set

Each round's base reel set is chosen by weight; the weighting is the main RTP tuning knob.

| Base reel set | Share of rounds | RTP when selected |
|---|---|---|
| `base_cellar` | 35.604% | 74.623% |
| `base_harvest` | 64.396% | 107.780% |

## Win distribution

| Round win (x bet) | Rounds | Frequency | 1 in | RTP contribution |
|---|---:|---:|---:|---:|
| 0 (no win) | 5,913,428 | 59.1343% | 1.7 | 0.000% |
| (0x, 1x) | 2,144,854 | 21.4485% | 4.7 | 7.998% |
| [1x, 2x) | 846,988 | 8.4699% | 11.8 | 10.908% |
| [2x, 5x) | 703,740 | 7.0374% | 14.2 | 20.633% |
| [5x, 10x) | 234,916 | 2.3492% | 42.6 | 15.710% |
| [10x, 20x) | 98,683 | 0.9868% | 101.3 | 12.647% |
| [20x, 50x) | 40,743 | 0.4074% | 245.4 | 11.800% |
| [50x, 100x) | 11,361 | 0.1136% | 880.2 | 7.845% |
| [100x, 250x) | 4,823 | 0.0482% | 2,073.4 | 6.877% |
| [250x, 500x) | 435 | 0.0044% | 22,988.5 | 1.374% |
| [500x, 1000x) | 28 | 0.0003% | 357,142.9 | 0.173% |
| [1000x, 2500x) | 1 | 0.0000% | 10,000,000.0 | 0.010% |
| [2500x, 5000x) | 0 | 0.0000% | - | 0.000% |
| 5000x (max win cap hit) | 0 | 0.0000% | - | 0.000% |

Standard deviation is per paid round (base spin plus any free spins it triggered), in multiples of the bet. The confidence interval is RTP ± 1.96·σ/√N.
