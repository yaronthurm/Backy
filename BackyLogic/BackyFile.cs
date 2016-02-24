using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class BackyFile
    {
        public string Name;
        public string FullName;
        public DateTime LastWriteTime;

        private BackyFile() { }

        public static BackyFile FromFullFileName(IFileSystem fileSystem, string fullname)
        {
            var ret = new BackyFile();
            ret.FullName = fullname;
            ret.LastWriteTime = fileSystem.GetLastWriteTime(fullname);
            ret.Name = System.IO.Path.GetFileName(fullname);
            return ret;
        }
    }
}
