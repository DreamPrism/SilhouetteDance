using System.Text.Json;

namespace SilhouetteDance.Configuration;

public class SilhouetteDanceConfig
{
    private static readonly string ConfigPath = "silhouette_dance_config.json";

    public string BotFilePath { get; init; }
    public string KeyStorePath=> Path.Combine(BotFilePath, "keystore.json");
    public string DeviceInfoPath => Path.Combine(BotFilePath, "device.json");
    public uint? BotUin { get; init; }
    public string BotPassword { get; init; }
    public static SilhouetteDanceConfig Instance { get; private set; }

    public static void Init()
    {
        if (!File.Exists(ConfigPath))
        {
            Instance = new SilhouetteDanceConfig
            {
                BotFilePath = "bot"
            };
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(Instance, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
        else
        {
            Instance = JsonSerializer.Deserialize<SilhouetteDanceConfig>(File.ReadAllText(ConfigPath));
        }

        EnsureDirectory(Instance.BotFilePath);
    }
    private static void EnsureDirectory(string path)
    {
        if (path == null || Directory.Exists(path)) return;
        Directory.CreateDirectory(path);
    }
}