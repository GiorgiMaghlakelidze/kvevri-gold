# Kvevri Gold RTP simulation: variant 94

Generated 2026-11-16 20:44 +04:00 by `KvevriGold.Simulator`.

| Setting | Value |
|---|---|
| Paid rounds | 10,000,000 |
| Seed | 20251116 (crypto RNG, 8 threads) |
| Base reel sets | `base_harvest` w=583, `base_cellar` w=417 |
| Free spins reel sets | `free_rtveli` w=1 |
| Runtime | 9.3s (1,076,829 rounds/s) |

## Results

| Metric | Value |
|---|---|
| **RTP** | **94.004%** |
| Target RTP | 94.000% (within ±0.3 pp, diff +0.004 pp) |
| 95% confidence interval | 93.678% to 94.331% (±0.327 pp) |
| Base game RTP contribution | 69.961% |
| Free spins RTP contribution | 24.043% |
| Hit frequency (round wins > 0) | 40.497% (1 in 2.5) |
| Free spins trigger frequency | 1 in 138.9 (0.720%) |
| Average free spins win | 33.40x bet |
| Average free spins played per trigger | 8.50 (retriggers: 4,216) |
| Standard deviation per round | 5.269x bet |
| Max win observed | 1,000.95x bet |
| Max win cap (5,000x) hits | 0 |

## RTP by base reel set

Each round's base reel set is chosen by weight; the weighting is the main RTP tuning knob.

| Base reel set | Share of rounds | RTP when selected |
|---|---|---|
| `base_cellar` | 41.676% | 74.512% |
| `base_harvest` | 58.324% | 107.932% |

## Win distribution

| Round win (x bet) | Rounds | Frequency | 1 in | RTP contribution |
|---|---:|---:|---:|---:|
| 0 (no win) | 5,950,253 | 59.5025% | 1.7 | 0.000% |
| (0x, 1x) | 2,141,328 | 21.4133% | 4.7 | 7.970% |
| [1x, 2x) | 835,491 | 8.3549% | 12.0 | 10.753% |
| [2x, 5x) | 693,247 | 6.9325% | 14.4 | 20.299% |
| [5x, 10x) | 229,273 | 2.2927% | 43.6 | 15.332% |
| [10x, 20x) | 94,683 | 0.9468% | 105.6 | 12.148% |
| [20x, 50x) | 39,385 | 0.3939% | 253.9 | 11.428% |
| [50x, 100x) | 11,087 | 0.1109% | 902.0 | 7.692% |
| [100x, 250x) | 4,779 | 0.0478% | 2,092.5 | 6.812% |
| [250x, 500x) | 445 | 0.0045% | 22,471.9 | 1.391% |
| [500x, 1000x) | 28 | 0.0003% | 357,142.9 | 0.169% |
| [1000x, 2500x) | 1 | 0.0000% | 10,000,000.0 | 0.010% |
| [2500x, 5000x) | 0 | 0.0000% | - | 0.000% |
| 5000x (max win cap hit) | 0 | 0.0000% | - | 0.000% |

Standard deviation is per paid round (base spin plus any free spins it triggered), in multiples of the bet. The confidence interval is RTP ± 1.96·σ/√N.
