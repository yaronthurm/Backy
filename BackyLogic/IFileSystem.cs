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
        void CreateFile(string filename);

        void Copy(string sourceFileName, string destFileName);

        IEnumerable<string> GetAllFiles(string dirName);

        IEnumerable<string> GetDirectories(string dirName);

        DateTime GetLastWriteTime(string fullname);

        IEnumerable<string> ReadLines(string fullname);

        void AppendLine(string filename, string line);

        byte[] GetContent(string physicalPath);
    }


    public class FileSystem : IFileSystem
    {
        public void AppendLine(string filename, string line)
        {
            File.AppendAllLines(filename, new[] { line });
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(destFileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFileName));

            File.Copy(sourceFileName, destFileName);
        }

        public void CreateFile(string filename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            File.WriteAllText(filename, "");
        }

        public IEnumerable<string> GetAllFiles(string source)
        {
            return Directory.GetFiles(source, "*", SearchOption.AllDirectories);
        }

        public byte[] GetContent(string physicalPath)
        {
            var ret = File.ReadAllBytes(physicalPath);
            return ret;
        }

        public IEnumerable<string> GetDirectories(string target)
        {
            return Directory.GetDirectories(target);
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            return File.GetLastWriteTime(fullname);
        }

        public IEnumerable<string> ReadLines(string filename)
        {
            var ret = File.ReadAllLines(filename);
            return ret;
        }
    }
}
