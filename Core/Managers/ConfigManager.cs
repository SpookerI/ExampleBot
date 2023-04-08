using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace ExampleBot.Core.Managers
{
    public static class ConfigManager
    {
        private static string ConfigFolder = "Config";
        private static string ConfigFile = "config.json";
        private static string ConfigPath = ConfigFolder + "/" + ConfigFile;
        public static BotConfig Config { get; private set; }

        static ConfigManager()
        {
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);
            if (!File.Exists(ConfigPath))
            {
                Config = new BotConfig();
                var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            else
            {
                var json = File.ReadAllText(ConfigPath);
                Config = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }
        public struct BotConfig
        {
            [JsonProperty("token")]
            public string Token { get; set; }
            [JsonProperty("prefix")]
            public string Prefix { get; set; }
            
        }
    }
}
