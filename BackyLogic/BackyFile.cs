using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class BackyFile
    {
        public string Root;
        public string RelativeName;
        public string PhysicalPath;
        public DateTime LastWriteTime;
        
        

        private BackyFile() { }

        public BackyFile Clone()
        {
            var ret = new BackyFile
            {
                LastWriteTime = this.LastWriteTime,
                PhysicalPath = this.PhysicalPath,
                RelativeName = this.RelativeName,
                Root = this.Root
            };
            return ret;
        }

        public static BackyFile FromSourceFileName(IFileSystem fileSystem, string fullname, string sourceDirectory)
        {
            var ret = new BackyFile();
            ret.Root = sourceDirectory;
            ret.RelativeName = fullname.Replace(sourceDirectory + "\\", "");
            ret.PhysicalPath = fullname;
            ret.LastWriteTime = fileSystem.GetLastWriteTime(fullname);
            return ret;
        }


        public static BackyFile FromTargetFileName(IFileSystem fileSystem, string fullname, string targetParentDirectory)
        {
            var ret = new BackyFile();
            ret.Root = targetParentDirectory;
            ret.RelativeName = fullname.Replace(ret.Root + "\\", "");
            ret.PhysicalPath = fullname;
            ret.LastWriteTime = fileSystem.GetLastWriteTime(fullname);
            return ret;
        }
    }
}

