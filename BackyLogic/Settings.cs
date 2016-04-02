using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class Settings
    {
        public string[] Sources { get; set; } = new string[0];
        public string Target { get; set; } = "";

        private Settings() { }

        private static string GetSettingsFilePath()
        {
            var settingsFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Backy",
                "Backy.settings");
            return settingsFile;
        }

        public static Settings Load()
        {
            var settingsFile = GetSettingsFilePath();
            if (!File.Exists(settingsFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(settingsFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(settingsFile));
                new Settings().Save();
            }

            var txt = File.ReadAllText(settingsFile);
            var ret = JsonConvert.DeserializeObject<Settings>(txt);
            return ret;
        }


        public void Save()
        {
            var txt = JsonConvert.SerializeObject(this);
            File.WriteAllText(GetSettingsFilePath(), txt);
        }

        public void SetSources(string[] sources)
        {
            this.Sources = sources;
        }

        public void SetTarget(string target)
        {
            this.Target = target;
        }
    }
}
