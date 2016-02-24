using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackyLogic
{
    public interface IFileSystem
    {
        void CreateDirectory(string targetDir);

        void Copy(string sourceFileName, string destFileName);

        IEnumerable<string> GetAllFiles(string source);

        IEnumerable<string> GetDirectories(string target);

        DateTime GetLastWriteTime(string fullname);
    }


    public class FileSystem : IFileSystem
    {
        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public void CreateDirectory(string targetDir)
        {
            Directory.CreateDirectory(targetDir);
        }

        public IEnumerable<string> GetAllFiles(string source)
        {
            return Directory.GetFiles(source, "*", SearchOption.AllDirectories);
        }

        public IEnumerable<string> GetDirectories(string target)
        {
            return Directory.GetDirectories(target);
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            return File.GetLastWriteTime(fullname);
        }
    }
}
