using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public class BackyFile: IVirtualFile
    {
        public string Root;
        public string RelativeName;
        public string PhysicalPath;
        public DateTime LastWriteTime;

        public string LogicalName
        {
            get
            {
                return RelativeName;
            }
        }

        string IVirtualFile.PhysicalPath
        {
            get
            {
                return PhysicalPath;
            }
        }

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
            ret.LastWriteTime = fileSystem.GetLastWriteTime(fullname);
            return ret;
        }

        public string[] GetPath()
        {
            return RelativeName.Split('\\');
        }
    }
}

