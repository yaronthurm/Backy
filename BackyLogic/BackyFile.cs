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

        public string Name;
        public string FullName;
        public DateTime LastWriteTime;

        private BackyFile() { }

        public static BackyFile FromSourceFileName(IFileSystem fileSystem, string fullname, string sourceDirectory)
        {
            var ret = new BackyFile();
            ret.Root = sourceDirectory;
            ret.RelativeName = fullname.Replace(sourceDirectory + "\\", "");
            ret.PhysicalPath = fullname;

            ret.FullName = fullname;
            ret.LastWriteTime = fileSystem.GetLastWriteTime(fullname);
            ret.Name = System.IO.Path.GetFileName(fullname);
            return ret;
        }

        /// <summary>
        /// This method takes into account the directories structure of Backy.
        /// So that if you backup to d:\target, the 
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="fullname"></param>
        /// <param name="sourceDirectory"></param>
        /// <returns></returns>
        public static BackyFile FromTargetFileName(IFileSystem fileSystem, string fullname, string targetParentDirectory)
        {
            var ret = new BackyFile();
            ret.Root = targetParentDirectory;
            ret.RelativeName = fullname.Replace(ret.Root + "\\", "");
            ret.PhysicalPath = fullname;

            ret.FullName = fullname;
            ret.LastWriteTime = fileSystem.GetLastWriteTime(fullname);
            ret.Name = System.IO.Path.GetFileName(fullname);
            return ret;
        }

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

