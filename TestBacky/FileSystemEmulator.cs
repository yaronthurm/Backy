using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBacky
{
    public class FileSystemEmulator : BackyLogic.IFileSystem
    {
        private List<string> _files;

        public FileSystemEmulator(IEnumerable<string> files)
        {
            _files = files.ToList();
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            _files.Add(destFileName);
        }

        public void CreateDirectory(string targetDir)
        {
        }

        public IEnumerable<string> GetAllFiles(string directory)
        {
            return _files.Where(x => x.StartsWith(directory));
        }

        public IEnumerable<string> GetDirectories(string directory)
        {
            var ret = this.GetAllFiles(directory).Select(Path.GetDirectoryName).Distinct().ToList();
            return ret;
        }

        public DateTime GetLastWriteTime(string fullname)
        {
            return new DateTime(2016, 1, 13, 13, 0, 0);
        }

        public IEnumerable<string> ReadLines(string fullname)
        {
            throw new NotImplementedException();
        }
    }
}
