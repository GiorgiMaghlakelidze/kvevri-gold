namespace KvevriGold.Simulator;

public sealed class SimOptions
{
    public string Variant { get; private set; } = "96";
    public long Spins { get; private set; } = 10_000_000;
    public ulong Seed { get; private set; } = 20251116;
    public int Threads { get; private set; } = Environment.ProcessorCount;
    public string RngKind { get; private set; } = "seeded";
    public string? ConfigPath { get; private set; }
    public string OutDir { get; private set; } = "reports";
    public bool Quiet { get; private set; }
    public bool Help { get; private set; }

    public static SimOptions Parse(string[] args)
    {
        var o = new SimOptions();
        for (int i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"Missing value for {args[i]}.");
            switch (args[i])
            {
                case "--variant": case "-v": o.Variant = Next(); break;
                case "--spins": case "-n": o.Spins = long.Parse(Next().Replace("_", "")); break;
                case "--seed": case "-s": o.Seed = ulong.Parse(Next()); break;
                case "--threads": case "-t": o.Threads = int.Parse(Next()); break;
                case "--rng": o.RngKind = Next().ToLowerInvariant(); break;
                case "--config": o.ConfigPath = Next(); break;
                case "--out": o.OutDir = Next(); break;
                case "--quiet": case "-q": o.Quiet = true; break;
                case "--help": case "-h": o.Help = true; break;
                default: throw new ArgumentException($"Unknown argument '{args[i]}'.");
            }
        }
        if (o.Spins <= 0) throw new ArgumentException("--spins must be positive.");
        if (o.Threads <= 0) throw new ArgumentException("--threads must be positive.");
        if (o.RngKind is not ("seeded" or "crypto")) throw new ArgumentException("--rng must be 'seeded' or 'crypto'.");
        return o;
    }

    public static void PrintUsage() => Console.WriteLine("""
        Usage: KvevriGold.Simulator [options]
          --variant, -v  94|96         RTP variant (default 96)
          --spins,   -n  N             paid rounds to simulate (default 10000000)
          --seed,    -s  N             base seed; thread t uses an independent stream (default 20251116)
          --threads, -t  N             worker threads (default: CPU count)
          --rng          seeded|crypto seeded = reproducible xoshiro256**, crypto = OS CSPRNG
          --config       path          use a JSON config instead of the embedded one
          --out          dir           report directory (default reports)
          --quiet, -q                  no progress output
        Results are reproducible for the same seed AND thread count when --rng seeded.
        """);
}
