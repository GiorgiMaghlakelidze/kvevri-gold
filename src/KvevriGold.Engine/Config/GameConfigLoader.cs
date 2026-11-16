using System.Text.Json;

namespace KvevriGold.Engine.Config;

public static class GameConfigLoader
{
    private const string ResourceName = "KvevriGold.Engine.Config.kvevri-gold.json";

    /// <summary>Loads the Kvevri Gold configuration that ships embedded in the engine assembly.</summary>
    public static GameConfig LoadEmbedded()
    {
        using Stream stream = typeof(GameConfigLoader).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");
        return Load(stream);
    }

    public static GameConfig LoadFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Load(stream);
    }

    public static GameConfig Load(Stream stream) =>
        JsonSerializer.Deserialize<GameConfig>(stream, GameConfig.JsonOptions)
        ?? throw new InvalidDataException("Game config is empty.");

    public static GameConfig Parse(string json) =>
        JsonSerializer.Deserialize<GameConfig>(json, GameConfig.JsonOptions)
        ?? throw new InvalidDataException("Game config is empty.");
}
