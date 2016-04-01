using System;
using System.Collections.Generic;
using System.IO;

namespace BackyLogic
{
    public class OSMachineIDProvider : IMachineIDProvider
    {
        public string GetOrCreateID()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var filePath = Path.Combine(appDataPath, "Backy", "Backy.ini");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                var ret = Guid.NewGuid().ToString("N");
                File.WriteAllText(filePath, ret);
                return ret;
            }
            else
            {
                var ret = File.ReadAllText(filePath);
                return ret;
            }
        }
    }

}
