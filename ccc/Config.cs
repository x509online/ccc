using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ccc
{
    class Config
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string ApiKey { get; set; }

        public void UpdateCrendentials(string userName, string apiKey)
        {
            this.UserName = userName;
            this.ApiKey = apiKey;
        }


        internal static string ConfigBasePath;
        internal static string ConfigFilePath; 

        private Config() { }

        public static Config Init()
        {
            ConfigBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ccc");

            ConfigFilePath = Path.Combine(ConfigBasePath, "ccc.config.json");
            InitConfigFolder();

            Config c = null;
            if (File.Exists(ConfigFilePath))
            {
                var jsonFile = File.ReadAllText(ConfigFilePath);
                c =  JsonConvert.DeserializeObject<Config>(jsonFile);
            }
            else
            {
                c = new Config { BaseUrl= Program.DEFAULT_BASE_URL };
                var updated = JsonConvert.SerializeObject(c);
                File.WriteAllText(ConfigFilePath, updated);
            }
            return c;
        }

        public void Flush()
        {
            var updated = JsonConvert.SerializeObject(this);
            File.WriteAllText(ConfigFilePath, updated);
        }

        private static void InitConfigFolder()
        {
            if (!Directory.Exists(ConfigBasePath))
            {
                Directory.CreateDirectory(ConfigBasePath);
            }
        }
    }
}
