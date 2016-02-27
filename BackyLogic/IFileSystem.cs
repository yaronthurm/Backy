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
    }


    public class FileSystem : IFileSystem
    {
        public void AppendLine(string filename, string line)
        {
            throw new NotImplementedException();
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public void CreateFile(string filename)
        {
            File.CreateText(filename);
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

        public IEnumerable<string> ReadLines(string filename)
        {
            return new string[0];
        }
    }
}
