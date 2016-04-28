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
        public Source[] Sources { get; set; } = new Source[0];
        public string Target { get; set; } = "";
        public string MachineID { get; set; } = Guid.NewGuid().ToString("N");

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
            ret.Sources = ret.Sources.Where(x => Directory.Exists(x.Path)).ToArray();
            return ret;
        }


        public void Save()
        {
            var txt = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(GetSettingsFilePath(), txt);
        }

        public void SetSources(IEnumerable<Source> sources)
        {
            this.Sources = sources.ToArray();
        }

        public void SetTarget(string target)
        {
            this.Target = target;
        }



        public class Source
        {
            public string Path;
            public bool Enabled;
        }
    }
}
